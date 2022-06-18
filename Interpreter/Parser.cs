namespace Basic.Interpreter
{

    // Converts the tokenized input into an AST.
    internal class Parser
    {

        private readonly List<Token> _tokens;
        private readonly Dictionary<string, UserStatement> _userStatements;
        private readonly Log _log;

        private int _current = 0;
        private long _lineNumber = 0;

        private readonly Dictionary<long, Stmt> _lineNumberIndex = new Dictionary<long, Stmt>();
        private readonly Dictionary<long, List<Token>> _data = new Dictionary<long, List<Token>>();
        private readonly Stack<List<long>> _lineNumberStack = new Stack<List<long>>();

        private List<long> _lineNumbersBeforeNextStatement = new List<long>();

        private readonly TokenType[] _endOfBlock = new[] { TokenType.Next };

        public Parser(Log log, List<Token> tokens, List<UserStatement> userStatements)
        {
            _log = log;
            _tokens = tokens;
            _userStatements = userStatements.ToDictionary(x => x.Label, x => x, StringComparer.InvariantCultureIgnoreCase);
        }

        public Ast Parse()
        {
            _current = 0;
            _lineNumber = 0;
            _data.Clear();
            _lineNumberIndex.Clear();
            _lineNumberStack.Clear();
            var statements = StatementsToEndOfBlock().ToList();
            return new Ast(statements, _lineNumberIndex, _data);
        }

        private IEnumerable<Stmt> StatementsToEndOfBlock(Stmt? previous = null)
        {

            _lineNumbersBeforeNextStatement.Clear();

            while (!IsAtEnd() && !_endOfBlock.Contains(Peek().Type))
            {

                if (Peek().Type == TokenType.Number)
                {

                    var labelToken = Advance();

                    if (!long.TryParse(labelToken.Lexeme, out _lineNumber))
                        throw Error(ErrorType.Syntax, labelToken, $"Bad line number {labelToken.Lexeme}");

                    _lineNumbersBeforeNextStatement.Add(_lineNumber);

                    if (IsAtEnd() || Match(TokenType.NewLine))
                        continue;

                    if (_endOfBlock.Contains(Peek().Type)) yield break;

                    previous = NextStatement(previous);
                    IndexStatement(previous);
                    yield return previous;

                }

                foreach (var s in StatementsToEndOfLine(previous))
                {
                    yield return s;
                    previous = s;
                }

            }
        }

        // If we start a FOR block we end up indexing all the lines inside
        // the block before creating the FOR statement itself, so push the FOR
        // block's line number onto a stack to remember when we're done creating
        private void PushLineNumber()
        {
            _lineNumberStack.Push(_lineNumbersBeforeNextStatement);
            _lineNumbersBeforeNextStatement = new List<long>();
        }

        private void PopLineNumber()
        {
            _lineNumbersBeforeNextStatement = _lineNumberStack.Pop();
        }

        private void IndexStatement(Stmt statement)
        {
            foreach (var l in _lineNumbersBeforeNextStatement)
            {
                _lineNumberIndex.Add(l, statement);
            }
            _lineNumbersBeforeNextStatement.Clear();
        }

        private IEnumerable<Stmt> IfLineBlock(Stmt? previous = null)
        {
            return NextStatementBlock(new[] { TokenType.Else, TokenType.NewLine }, Array.Empty<TokenType>(), previous);
        }

        private IEnumerable<Stmt> NextStatementBlock(TokenType[] stopBefore, TokenType[] stopAt, Stmt? previous = null)
        {
            while (!IsAtEnd() && !stopBefore.Contains(Peek().Type) && !Match(stopAt))
            {
                previous = NextStatement(previous);
                yield return previous;
            }
        }

        private IEnumerable<Stmt> StatementsToEndOfLine(Stmt? previous)
        {
            while (!IsAtEnd() && !_endOfBlock.Contains(Peek().Type) && !Match(TokenType.NewLine))
            {
                previous = NextStatement(previous);
                yield return previous;
            }
        }

        // Return the next statement at or after the cursor, then advance the
        // cursor to the next statement or end of line token.
        private Stmt NextStatement(Stmt? prev)
        {

            while (Peek().Type == TokenType.Colon) Advance();
            Stmt stmt;
            try
            {
                stmt = Statement();
            }
            catch (ParseError)
            {
                stmt = new Stmt.Noop();
                while (!EndOfStatement()) Advance();
            }

            if (prev is not null)
            {
                prev.SetNextStatement(stmt);
            }

            while (Peek().Type == TokenType.Colon) Advance();

            return stmt;

        }

        private Stmt Statement()
        {

            if (Match(TokenType.Print)) return PrintStatement();
            if (Match(TokenType.UserStatement)) return UserStatement();
            if (Match(TokenType.If)) return IfStatement();
            if (Match(TokenType.Run)) return new Stmt.Run();
            if (Match(TokenType.Goto)) return GotoStatement();
            if (Match(TokenType.Gosub)) return GosubStatement();
            if (Match(TokenType.Return)) return new Stmt.Return(Previous());
            if (Match(TokenType.For)) return ForStatement();
            if (Match(TokenType.End)) return new Stmt.End();
            if (Match(TokenType.Dim)) return DimStatement();
            if (Match(TokenType.Data)) return DataStatement();
            if (Match(TokenType.Read)) return ReadStatement();
            if (Match(TokenType.Rem)) return RemStatement();
            if (Match(TokenType.On)) return OnStatement();
            if (Match(TokenType.New)) return new Stmt.New();

            if (Match(TokenType.Else)) throw Error(ErrorType.Block, Previous(), "Unexpected 'ELSE'.");
            if (Match(TokenType.Next)) throw Error(ErrorType.Block, Previous(), "Unexpected 'NEXT'.");

            // Add more here

            // Check for assignment 
            var message = "Expected statement or assignment.";
            var token = Peek();
            if (token.Type == TokenType.Let)
            {
                message = "Expected assignment target.";
                token = Advance();
            }

            var expr = Primary();

            if (expr is Expr.Variable variable)
            {
                if (Match(TokenType.Equal))
                {
                    var value = Expression();
                    return new Stmt.Assign(variable, value);
                }
            }

            throw Error(ErrorType.Syntax, token, message);
        }

        private Stmt UserStatement()
        {
            var token = Previous();
            var lexeme = token.Lexeme;
            var userStatement = (GenericUserStatement)_userStatements[lexeme];

            var collection = userStatement.Tokens.ToList();
            var collectionIndex = Enumerable.Range(0, collection.Count).ToList();

            int index = 0;
            var expressionBag = new List<Expr>();
            var expressionToken = new List<Token>();
            while (true)
            {
                var t = collection[0][index];
                if (t.Type == TokenType.Identifier)
                {
                    foreach (var c in collection)
                        if (c[index].Type != TokenType.Identifier) Error(ErrorType.Syntax, token, $"Custom keyword, index {index}, cannot mix identifiers and non-identifiers in patterns.");
                    expressionToken.Add(Peek());
                    var expr = Expression();
                    expressionBag.Add(expr);

                }
                else
                {
                    var expected = new List<TokenType>();
                    for (int i = collection.Count - 1; i >= 0; i--)
                    {
                        var q = collection[i][index];
                        expected.Add(q.Type);
                        if (q.Type == TokenType.Identifier) Error(ErrorType.Syntax, token, $"Custom keyword, index {index}, cannot mix identifiers and non-identifiers in patterns.");
                        
                        if (  !(q.Type == Peek().Type || (q.Type == TokenType.EOF && EndOfStatement())))
                        {
                            collection.RemoveAt(i);
                            collectionIndex.RemoveAt(i);
                        }
                    }

                    if (!EndOfStatement()) Advance();

                    if (collection.Count == 0)
                        Error(ErrorType.Syntax, token, "Expected: " + string.Join(", ", expected.Distinct()));

                }

                index++;

                if (collection.Count == 1 && collection[0].Length == index) break;

            }

            int j = 0;
            foreach (var literal in collection[0].Where(x => x.Type == TokenType.Identifier))
            {
                if (literal.Lexeme.StartsWith('I') && expressionBag[j] is not Expr.Variable)
                    Error(ErrorType.Syntax, expressionToken[j], "Expected variable.");
                j++;
            }
            return new Stmt.User(token, userStatement, collectionIndex[0], expressionBag);

        }

        private Stmt OnStatement()
        {
            var token = Previous();
            var expr = Expression();

            var keyword = Consume(ErrorType.Syntax, "Expected GOTO or GOSUB.", TokenType.Goto, TokenType.Gosub);

            var targets = new List<Expr>();

            do
            {
                targets.Add(Expression());
            } while (Match(TokenType.Comma));

            return new Stmt.On(token, expr, keyword, targets);

        }

        private Stmt RemStatement()
        {
            while (Match(TokenType.Comment)) { }
            return new Stmt.Noop();
        }

        private Stmt ReadStatement()
        {
            var variables = new List<Expr.Variable>();
            do
            {
                var token = Peek();
                var expr = Primary();
                if (expr is not Expr.Variable variable)
                    throw Error(ErrorType.Syntax, token, "Expected variable.");
                variables.Add(variable);
            } while (Match(TokenType.Comma));
            return new Stmt.Read(variables);
        }

        private Stmt DataStatement()
        {

            if (!_data.TryGetValue(_lineNumber, out var dataLine))
            {
                dataLine = new List<Token>();
                _data[_lineNumber] = dataLine;
            }

            while (!EndOfStatement())
            {
                var data = Consume(ErrorType.Syntax, "Expected data literal.", TokenType.DataLiteral);

                dataLine.Add(data);

                if (!EndOfStatement()) Consume(ErrorType.Syntax, "Expected ',' or end of statement", TokenType.Comma);
            }

            return new Stmt.Noop();

        }

        private Stmt DimStatement()
        {
            
            var variables = new List<Expr.Variable>();

            do
            {
                var token = Peek();
                var primary = Primary();
                if (primary is not Expr.Variable variable)
                    throw Error(ErrorType.Syntax, token, "Expected variable.");
                variables.Add(variable);
            } while (Match(TokenType.Comma));

            return new Stmt.Dim(variables);

        }

        private Stmt ForStatement()
        {
            var token = Previous();
            var variableToken = Peek();
            var primary = Primary();
            if (!(primary is Expr.Variable variable))
                throw Error(ErrorType.Syntax, Previous(), "Expected variable.");

            if (variable.Parameters.Count > 0)
                throw Error(ErrorType.Syntax, variableToken, "Array variable not allowed in FOR.");

            Consume(ErrorType.Syntax, "Expected '='.", TokenType.Equal);
            var start = Expression();
            Consume(ErrorType.Syntax, "Expected 'TO'.", TokenType.To);
            var end = Expression();

            var step = (Match(TokenType.Step))
             ? Expression()
             : new Expr.Literal(1);

            ConsumeEndOfStatement();

            PushLineNumber();

            var block = StatementsToEndOfBlock().ToList();
            if (block.Count == 0) block.Add(new Stmt.Noop());

            Consume(ErrorType.Block, "Expected NEXT", TokenType.Next);

            if (Peek().Type == TokenType.Identifier)
            {
                var vTok = Advance();
                if (!vTok.Lexeme.Equals(variable.Name.Lexeme, StringComparison.InvariantCultureIgnoreCase))
                {
                    var message = "Counter mismatched with FOR " + variable.Name.Lexeme;
                    if (variable.Name.Line > 0) message += $" on line {variable.Name.Line}";
                    throw Error(ErrorType.Block, vTok, message);
                }

            }

            var forStatement = new Stmt.For(token, variable, start, end, step, block);

            var nextStatement = new Stmt.Next(forStatement);
            IndexStatement(nextStatement);
            block.Last().SetNextStatement(nextStatement);

            PopLineNumber();
            return forStatement;

        }

        private Stmt GotoStatement()
        {
            var token = Previous();
            var expr = Expression();
            return new Stmt.Goto(token, expr);
        }

        private Stmt GosubStatement()
        {
            var token = Previous();
            var expr = Expression();
            return new Stmt.Gosub(token, expr);
        }

        private Stmt IfStatement()
        {
            var expr = Expression();
            Consume(ErrorType.Syntax, "Expected 'THEN'.", TokenType.Then);

            var trueStmt = Peek().Type == TokenType.Number
             ? new List<Stmt> { new Stmt.Goto(Peek(), Primary()) }
             : IfLineBlock().ToList();

            var falseStmt = (Match(TokenType.Else)
                ? IfLineBlock()
                : Enumerable.Empty<Stmt>()).ToList();

            if (Peek().Type == TokenType.Else) throw Error(ErrorType.Block, Peek(), "Unexpected 'ELSE'.");

            return new Stmt.If(expr, trueStmt, falseStmt);

        }

        private Stmt PrintStatement()
        {
            var token = Previous();
            var lineBreak = true;
            var expressions = new List<Expr>();
            while (!EndOfStatement())
            {
                if (Match(TokenType.Semicolon))
                {
                    lineBreak = false;
                }
                else if (Match(TokenType.Comma))
                {
                    lineBreak = false;
                    expressions.Add(new Expr.Literal(" "));
                }
                else
                {
                    lineBreak = true;
                    expressions.Add(Expression());
                }
            }

            var userStatement = (PrintUserStatement)_userStatements[token.Lexeme];
            return new Stmt.Print(userStatement, token, expressions, lineBreak);
        }

        private Expr Expression()
        {
            return And();
        }


        private Expr And()
        {
            var expr = Xor();
            while (Match(TokenType.And))
            {
                var @operator = Previous();
                var right = Xor();
                expr = new Expr.Binary(expr, @operator, right);
            }
            return expr;
        }

        private Expr Xor()
        {
            var expr = Or();
            while (Match(TokenType.Xor))
            {
                var @operator = Previous();
                var right = Or();
                expr = new Expr.Binary(expr, @operator, right);
            }
            return expr;
        }

        private Expr Or()
        {
            var expr = Equality();
            while (Match(TokenType.Or))
            {
                var @operator = Previous();
                var right = Equality();
                expr = new Expr.Binary(expr, @operator, right);
            }
            return expr;
        }
        
        private Expr Equality()
        {
            var expr = Comparison();
            while (Match(TokenType.Equal, TokenType.NotEqual))
            {
                var @operator = Previous();
                var right = Comparison();
                expr = new Expr.Binary(expr, @operator, right);
            }
            return expr;
        }

        private Expr Comparison()
        {
            var expr = Term();

            while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                var @operator = Previous();
                var right = Term();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr Term()
        {
            var expr = Factor();

            while (Match(TokenType.Minus, TokenType.Plus))
            {
                var @operator = Previous();
                var right = Factor();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr Factor()
        {
            var expr = Unary();

            while (Match(TokenType.Slash, TokenType.Star))
            {
                var @operator = Previous();
                var right = Unary();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(TokenType.Minus, TokenType.Not))
            {
                var @operator = Previous();
                var right = Unary();
                return new Expr.Unary(@operator, right);
            }
            return Primary();
        }

        private Expr Primary()
        {

            if (Match(TokenType.Number, TokenType.String))
                return new Expr.Literal(Previous().Literal);

            if (Match(TokenType.Identifier))
            {
                var parameters = new List<Expr>();
                var name = Previous();
                if (Match(TokenType.LeftParen) && !Match(TokenType.RightParen))
                {
                    while (true)
                    {
                        parameters.Add(Expression());
                        if (Peek().Type == TokenType.RightParen)
                        {
                            Advance();
                            break;
                        }
                        Consume(ErrorType.Syntax, "Expected ',' or ')'.", TokenType.Comma);
                    }

                }
                return new Expr.Variable(name, parameters);
            }

            if (Match(TokenType.LeftParen))
            {
                var expr = Expression();
                Consume(ErrorType.Syntax, "Expect ')' after expression", TokenType.RightParen);
                return new Expr.Grouping(expr);
            }

            throw Error(ErrorType.Syntax, Peek(), "Expected expression");

        }

        // Check if current token matches any in the list and advance to the
        // next token if so.
        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(params TokenType[] types)
        {
            if (IsAtEnd()) return false;
            return types.Any(type => Peek().Type == type);
        }

        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Token Peek()
        {
            return GetToken(_current);
        }

        private Token Previous()
        {
            return GetToken(_current - 1);
        }

        private Token GetToken(int index)
        {
            return new Token(_tokens[index].Type, _tokens[index].Lexeme, _tokens[index].Literal, _lineNumber);
        }

        private bool EndOfStatement()
        {
            return IsAtEnd() || Check(TokenType.Colon, TokenType.NewLine, TokenType.Else);

        }

        private Token ConsumeEndOfStatement()
        {
            if (IsAtEnd()) return Peek();
            return Consume(ErrorType.Syntax, "Expected end of statement.", TokenType.NewLine, TokenType.Colon);
        }

        private Token Consume(ErrorType errorType, string message, params TokenType[] types)
        {
            if (Check(types)) return Advance();
            throw Error(errorType, Peek(), message);
        }

        private ParseError Error(ErrorType errorType, Token token, string message)
        {
            _log.Error(errorType, token, message);
            return new ParseError();
        }

    }
}

