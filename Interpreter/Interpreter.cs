using System;
using System.Text;
using Basic.Interpreter.NativeFunctions;
using System.Linq;
using static Basic.Interpreter.UserStatement;

namespace Basic.Interpreter
{
	public class Interpreter : Expr.Visitor<object>, Stmt.Visitor<Stmt>, IContext
	{

        private Environment _environment;
        private Ast _ast = new Ast(new List<Stmt>(), new Dictionary<long, Stmt>(), new Dictionary<long, List<Token>>());
        private Dictionary<long, List<Token>> _program = new Dictionary<long, List<Token>>();
        private readonly Stack<Stmt> _returnStack = new Stack<Stmt>();
        private readonly List<PrintUserStatement> _printStatements = new List<PrintUserStatement>();
        private readonly List<GenericUserStatement> _genericStatements = new List<GenericUserStatement>();

        private readonly List<KeyValuePair<string, IFunction>> _userFunctions = new List<KeyValuePair<string, IFunction>>();


        public Interpreter()
		{
            _environment = new Environment();
            ResetVariables();
        }

        public void AddFunction(IFunction function, string label)
        {
            var kvp = new KeyValuePair<string, IFunction>(label.ToUpperInvariant(), function);
            _userFunctions.Add(kvp);
            _environment.Define(kvp.Key, kvp.Value);
        }

        private static readonly Dictionary<string, IFunction> _nativeFunctions = new Dictionary<string, IFunction>
        {
            { "RND", new Rnd() },
            { "INT", new Int()},

            { "SQR", new Sqr() },

            { "SIN", new Sin()},
            { "COS", new Cos()},
            { "TAN", new Tan()},
            { "ASIN", new Asin()},
            { "ACOS", new Acos()},
            { "ATAN", new Atan()},

            { "ABS", new Abs()},

            { "MID$", new Mid()},
            { "LEFT$", new Left()},
            { "RIGHT$", new Right()},
            { "ASC", new Asc()},
            { "CHR$", new Chr()},
            { "LEN", new Len()}
        };

        private void ResetVariables()
        {
            _environment = new Environment();
            foreach (var kvp in _nativeFunctions)
            {
                _environment.Define(kvp.Key, kvp.Value);
            }

            foreach (var kvp in _userFunctions)
            {
                _environment.Define(kvp.Key, kvp.Value);
            }

        }


        public void AddStatement(GenericUserStatement input)
        {
            _genericStatements.Add(input);

        }

        public void AddPrintStatement(PrintUserStatement print)
        {
            _printStatements.Add(print);
        }

        private IEnumerable<UserStatement> UserStatements()
        {
            return _genericStatements.Concat<UserStatement>(_printStatements);
        }

        private IEnumerable<KeyValuePair<string, TokenType>> UserKeywords()
        {
            return UserStatements().Select(x => new KeyValuePair<string, TokenType>(x.Label, x.TokenType));
        }

        public IEnumerable<string> Keywords()
        {
            var scanner = new Scanner(new Log(), "", UserKeywords());
            return scanner.Keywords.Union(_nativeFunctions.Keys).Union(_userFunctions.Select(x => x.Key));
        }

        public void Immediate(string line)
        {

            if (line is null || line.Trim() == "") return;

            var log = new Log();
            var scanner = new Scanner(log, line, UserKeywords());

            var tokens = scanner.Tokens();

            if (log.Any())
            {
                PrintLog(log);
                return;
            }

            var parser = new Parser(log, tokens, UserStatements().ToList());
            var expression = parser.Parse().Statements;

            if (tokens[0].Type == TokenType.Number)
            {

                if (tokens[0].Literal is not long lineNumber || lineNumber < 1)
                    log.Error(ErrorType.Syntax, 0, "Bad line number.");

                if (log.Any(x => x.ErrorType != ErrorType.Block))
                {
                    PrintLog(log, x => x.ErrorType != ErrorType.Block);
                    return;
                }

                _program[(long)tokens[0].Literal] = tokens
                    .Select(x => x.Type == TokenType.EOF
                                    ? new Token(TokenType.NewLine, "\n", '\n', x.Line)
                                    : x)
                    .ToList();
            }
            else
            {

                if (log.Any())
                {
                    PrintLog(log);
                    return;
                }

                try
                {
                    var next = expression[0];
                    while (next is not Stmt.End)
                    {
                        next = next.Accept(this);
                    }
                }
                catch (RuntimeError e)
                {
                    var message = "";
                    if (e.Token.Line > 0) message += $"[line {e.Token.Line}] ";
                    if (e.Token.Type == TokenType.EOF)
                        message += $"Error at end: ";
                    else
                        message += $"Error at {e.Token.Lexeme}: ";
                    message += e.Message;
                    Console.Error.WriteLine(message);
                }
                
            }
            
        }

        private void PrintLog(Log log, Predicate<ErrorMessage>? predicate = null)
        {
            var p = predicate ?? (x => true);
            foreach (var m in log.PopMessages().Where(x => p(x)))
            {
                Console.Error.WriteLine(m.Message);
            }
        }

        object Expr.Visitor<object>.VisitBinaryExpr(Expr.Binary expr)
        { 

            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);
            var op = expr.Operator.Type;

            try
            {
                return Evaluate(left, op, right);
            }
            catch (TokenlessRuntimeError e)
            {
                throw new RuntimeError(expr.Operator, e.Message);
            }

        }

        private object Evaluate(object left, TokenType op, object right)
        {
            if (left is long li && right is long ri)
            {
                return Evaluate(li, op, ri);
            }
            else if (left is string ls && right is string rs)
            {
                return Evaluate(ls, op, rs);
            }
            else
            {
                return Evaluate(Convert.ToDouble(left), op, Convert.ToDouble(right));
            }
        }

        private long Evaluate(long left, TokenType op, long right)
        {
            switch (op)
            {

                case TokenType.Equal:
                    return left == right ? -1 : 0;
                case TokenType.NotEqual:
                    return left != right ? -1 : 0;
                case TokenType.Greater:
                    return left > right ? -1 : 0;
                case TokenType.GreaterEqual:
                    return left >= right ? -1 : 0;
                case TokenType.Less:
                    return left < right ? -1 : 0;
                case TokenType.LessEqual:
                    return left <= right ? -1 : 0;
                case TokenType.Minus:
                    checked
                    {
                        return left - right;
                    }
                case TokenType.Plus:
                    checked
                    {
                        return left + right;
                    }
                case TokenType.Slash:
                    checked
                    {
                        return left / right;
                    }
                case TokenType.Star:
                    checked
                    {
                        return left * right;
                    }
                case TokenType.And:
                    return left & right;

                case TokenType.Or:
                    return left | right;

                case TokenType.Xor:
                    return left ^ right;
            }

            throw new TokenlessRuntimeError("Operator invalid for integer operand.");

        }

        private object Evaluate(double left, TokenType op, double right)
        {
            switch (op)
            {

                case TokenType.Equal:
                    return left == right ? -1L : 0L;
                case TokenType.NotEqual:
                    return left != right ? -1L : 0L;
                case TokenType.Greater:
                    return left > right ? -1L : 0L;
                case TokenType.GreaterEqual:
                    return left >= right ? -1L : 0L;
                case TokenType.Less:
                    return left < right ? -1L : 0L;
                case TokenType.LessEqual:
                    return left <= right ? -1L : 0L;
                case TokenType.Minus:
                    checked
                    {
                        return left - right;
                    }
                case TokenType.Plus:
                    checked
                    {
                        return left + right;
                    }
                case TokenType.Slash:
                    checked
                    {
                        return left / right;
                    }
                case TokenType.Star:
                    checked
                    {
                        return left * right;
                    }
            }

            throw new TokenlessRuntimeError("Operator invalid for floating point operand.");

        }


        private object Evaluate(string left, TokenType op, string right)
        {
            switch (op)
            {

                case TokenType.Equal:
                    return left == right ? -1L : 0L;
                case TokenType.NotEqual:
                    return left != right ? -1L : 0L;
                case TokenType.Plus:
                    checked
                    {
                        return left + right;
                    }
            }

            throw new TokenlessRuntimeError("Operator invalid for string operand.");

        }

        object Expr.Visitor<object>.VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        object Expr.Visitor<object>.VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value;
        }

        object Expr.Visitor<object>.VisitUnaryExpr(Expr.Unary expr)
        {
            var right = Evaluate(expr.Right);
            
            switch (expr.Operator.Type)
            {
                case TokenType.Minus:
                    {
                        if (right is double d) return -d;
                        if (right is long i) return -i;
                        throw new RuntimeError(expr.Operator, "Operand must be a number.");
                    }
                case TokenType.Not:
                    {
                        if (right is long i) return ~i;
                        throw new RuntimeError(expr.Operator, "Operand must be an integer.");
                    }
            }

            throw new RuntimeError(expr.Operator, "Unexpected token.");
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private string Stringify(object value)
        {
            return value?.ToString() ?? "NULL";
        }

        Stmt Stmt.Visitor<Stmt>.VisitPrintStmt(Stmt.Print print)
        {
            var sb = new StringBuilder();
            foreach (var stmt in print.Expressions)
                sb.Append(Stringify(Evaluate(stmt)));

            if (print.NewLine) sb.AppendLine();

            try
            {
                var result = print.UserStatement.Execute(sb.ToString());
                if (result is UserStatement.StatementResultError e)
                {
                    throw new RuntimeError(print.Stmt, e.Message);
                }
            }
            catch (TokenlessRuntimeError e)
            {
                throw new RuntimeError(print.Stmt, e.Message);
            }

            return print.NextStatement;
        }

        Stmt Stmt.Visitor<Stmt>.VisitLetStmt(Stmt.Let stmt)
        {
            var value = Evaluate(stmt.Initializer);
            _environment.Define(stmt.Name.Lexeme, value);
            return stmt.NextStatement;
        }

        object Expr.Visitor<object>.VisitVariableExpr(Expr.Variable expr)
        {

            if (_environment.IsFunction(expr.Name, out var fn))
            {
                if (fn?.AcceptsArity(expr.Parameters.Count) ?? false)
                {
                    try
                    {
                        var parameters = expr.Parameters.Select(x => Evaluate(x)).ToArray();
                        return fn.Call(parameters);
                    }
                    catch (TokenlessRuntimeError e)
                    {
                        throw new RuntimeError(expr.Name, e.Message);
                    }
                }
                throw new RuntimeError(expr.Name, $"Invalid arity: {expr.Parameters.Count()}.");
            }

            return ReadVariable(expr);
        }

        Stmt Stmt.Visitor<Stmt>.VisitAssignStmt(Stmt.Assign stmt)
        {
            var value = Evaluate(stmt.Value);
            var element = ResolveVariable(stmt.Variable);
            _environment.Assign(element, value);
            return stmt.NextStatement;
        }

        Stmt Stmt.Visitor<Stmt>.VisitNoopStmt(Stmt.Noop stmt)
        {
            return stmt.NextStatement;
        }

        Stmt Stmt.Visitor<Stmt>.VisitIfStmt(Stmt.If ifStmt)
        {

            var stmts = ((long)Evaluate(ifStmt.Condition) != 0)
                ? ifStmt.IfTrue
                : ifStmt.IfFalse;

            if (stmts.Any()) return stmts[0];
            return ifStmt.NextStatement;

        }

        Stmt Stmt.Visitor<Stmt>.VisitRunStmt(Stmt.Run runStmt)
        {
            ResetVariables();
            _returnStack.Clear();

            var tokens = _program
                .OrderBy(x => x.Key)
                .Select(x => x.Value)
                .SelectMany(x => x)
                .ToList();

            if (!tokens.Any()) return new Stmt.End();

            var line = tokens.Any() ? tokens.Last().Line : 0;
            tokens.Add(new Token(TokenType.EOF, "\0", '\0', line));

            var log = new Log();

            var parser = new Parser(log, tokens, UserStatements().ToList());
            _ast = parser.Parse();

            if (log.Any())
            {
                PrintLog(log);
                return new Stmt.End();
            }

            if (!_ast.Statements.Any()) return new Stmt.End();

            return _ast.Statements[0];
        }

        Stmt Stmt.Visitor<Stmt>.VisitGotoStmt(Stmt.Goto stmt)
        {
            try
            {
                var destination = Evaluate(stmt.Label);
                return _ast.StatementIndex((long)destination);
            }
            catch
            {
                throw new RuntimeError(stmt.Stmt, "Invalid label.");
            }
        }

        Stmt Stmt.Visitor<Stmt>.VisitGosubStmt(Stmt.Gosub gosub)
        {
            try
            {
                var destination = Evaluate(gosub.Label);
                _returnStack.Push(gosub.NextStatement);
                return _ast.StatementIndex((long)destination);
            }
            catch
            {
                throw new RuntimeError(gosub.Stmt, "Invalid label.");
            }
        }

        Stmt Stmt.Visitor<Stmt>.VisitForStmt(Stmt.For forStmt)
        {

            var variable = ResolveVariable(forStmt.Variable);

            var initial = Evaluate(forStmt.Start);
            var step = Evaluate(forStmt.Step);
            var target = Evaluate(forStmt.Target);

            _environment.Assign(variable, initial);

            return DirectForNext(forStmt, initial, target, step);
        }

        Stmt Stmt.Visitor<Stmt>.VisitNextStmt(Stmt.Next nextStmt)
        {

            var forStmt = nextStmt.ForStatement;

            var variable = forStmt.Variable;
            var step = Evaluate(forStmt.Step);
            var value = Evaluate(ReadVariable(variable), TokenType.Plus, step);
            var target = Evaluate(forStmt.Target);
            _environment.Assign(ResolveVariable(variable), value);

            return DirectForNext(forStmt, value, target, step);
            
        }

        private Stmt DirectForNext(Stmt.For forStmt, object value, object target, object step)
        {
            if (EvaluateComparison(step, TokenType.Greater, 0) && EvaluateComparison(value, TokenType.Greater, target)
               || EvaluateComparison(step, TokenType.Less, 0) && EvaluateComparison(value, TokenType.Less, target))
                return forStmt.NextStatement;

            return forStmt.Block[0];
        }

        private bool EvaluateComparison(object left, TokenType op, object right)
        {
            return (long)Evaluate(left, op, right) != 0;
        }

        private object ReadVariable(ResolvedVariable variable)
        {
            return _environment.Get(variable);
        }

        private object ReadVariable(Expr.Variable variable)
        {
            return _environment.Get(ResolveVariable(variable));
        }

        private void WriteVariable(ResolvedVariable variable, object value)
        {
            _environment.Assign(variable, value);
        }

        private void WriteVariable(Expr.Variable variable, object value)
        {
            _environment.Assign(ResolveVariable(variable), value);
        }

        public string Listing()
        {
            var sb = new StringBuilder();

            var padRight = false;
            var lastToken = TokenType.NewLine;
            foreach (var token in _program.OrderBy(x => x.Key).Select(x => x.Value).SelectMany(x => x))
            {

                switch (token.Type)
                {

                    case TokenType.NewLine:
                    case TokenType.EOF:
                        sb.AppendLine();
                        padRight = false;
                        break;

                    case TokenType.Dot:
                    case TokenType.LeftParen:
                    case TokenType.RightParen:
                        if (padRight) sb.Append(" ");
                        sb.Append(token.Lexeme);
                        padRight = false;
                        break;

                    case TokenType.Minus:
                    case TokenType.Plus:
                    case TokenType.Slash:
                    case TokenType.Star:
                    case TokenType.NotEqual:
                    case TokenType.Equal:
                    case TokenType.Greater:
                    case TokenType.GreaterEqual:
                    case TokenType.Less:
                    case TokenType.LessEqual:
                        sb.Append(" " + token.Lexeme);
                        padRight = true;
                        break;

                    case TokenType.Colon:
                    case TokenType.Semicolon:
                    case TokenType.Comma:
                        if (padRight) sb.Append(" ");
                        sb.Append(token.Lexeme);
                        padRight = true;
                        break;

                    case TokenType.Identifier:
                        if (padRight) sb.Append(" ");
                        sb.Append(token.Lexeme);
                        padRight = false;
                        break;

                    case TokenType.Comment:
                    case TokenType.DataLiteral:
                        if (padRight) sb.Append(" ");
                        sb.Append(token.Literal);
                        padRight = false;
                        break;

                    case TokenType.String:
                    case TokenType.Number:
                        if (padRight) sb.Append(" ");
                        sb.Append(token.Lexeme);
                        padRight = (token.Type == TokenType.Number && lastToken == TokenType.NewLine);
                        break;

                    default:
                        sb.Append(" " + token.Lexeme);
                        padRight = true;
                        break;


                }

                lastToken = token.Type;

            }

            return sb.ToString();

        }
        
        Stmt Stmt.Visitor<Stmt>.VisitEndStmt(Stmt.End stmt)
        {
            throw new NotImplementedException();
        }

        Stmt Stmt.Visitor<Stmt>.VisitReturnStmt(Stmt.Return stmt)
        {
            if (!_returnStack.TryPop(out var returnTo))
            {
                throw new RuntimeError(stmt.Stmt, "RETURN without GOSUB.");
            }

            return returnTo;
        }

        Stmt Stmt.Visitor<Stmt>.VisitDimStmt(Stmt.Dim dim)
        {
            foreach (var variable in dim.Variable)
            {
                try
                {
                    var r = ResolveVariable(variable);
                    _environment.Define(r.Token.Lexeme, r.Element);
                }
                catch (TokenlessRuntimeError e)
                {
                    throw new RuntimeError(variable.Name, e.Message);
                }
            }

            return dim.NextStatement;
        }

        private ResolvedVariable ResolveVariable(Expr.Variable variable)
        {
            long[] element;
            try
            {
                element = variable.Parameters.Select(p => Evaluate(p)).Select(v => Converters.ToInt(v)).ToArray();
            }
            catch (TokenlessRuntimeError e)
            {
                throw new RuntimeError(variable.Name, e.Message);
            }
            return new ResolvedVariable(variable.Name, element);
        }

        Stmt Stmt.Visitor<Stmt>.VisitReadStmt(Stmt.Read stmt)
        {
            foreach (var variable in stmt.Variables)
            {
                string value = "";
                try
                {
                    value = _ast.FetchDataLiteral();
                }
                catch (TokenlessRuntimeError e)
                {
                    throw new RuntimeError(variable.Name, e.Message);
                }

                var resolvedVariable = ResolveVariable(variable);

                var existingValue = ReadVariable(resolvedVariable);

                if (existingValue is long)
                {
                    if (!long.TryParse(value, out var i))
                        throw new RuntimeError(variable.Name, $"Type mismatch reading data '{value}'.");
                    WriteVariable(resolvedVariable, i);
                }
                else if (existingValue is double)
                {
                    if (!double.TryParse(value, out var d))
                        throw new RuntimeError(variable.Name, $"Type mismatch reading data '{value}'.");
                    WriteVariable(resolvedVariable, d);
                }
                else if (existingValue is string)
                {
                    WriteVariable(resolvedVariable, value);
                }
                else
                {
                    throw new Exception();
                }
            }

            return stmt.NextStatement;
        }

        private long ToInt(object value, Token tokenIfError)
        {
            try
            {
                return Converters.ToInt(value);
            }
            catch
            {
                throw new RuntimeError(tokenIfError, "Type mismatch.");
            }
        }

        Stmt Stmt.Visitor<Stmt>.VisitOnStmt(Stmt.On stmt)
        {
            var index = ToInt(Evaluate(stmt.Expression), stmt.Name);
            if (index < 1 || index > stmt.Targets.Count || index > int.MaxValue) return stmt.NextStatement;
            var target = stmt.Targets[(int)index - 1];
            var lineNumber = ToInt(Evaluate(target), stmt.Name);
            var nextStatement = _ast.StatementIndex(lineNumber);
            switch (stmt.Keyword.Type)
            {
                case TokenType.Goto:
                    return nextStatement;
                case TokenType.Gosub:
                    _returnStack.Push(stmt.NextStatement);
                    return nextStatement;
                default:
                    throw new RuntimeError(stmt.Keyword, "Expected GOTO or GOSUB.");
            }
        }

        Stmt Stmt.Visitor<Stmt>.VisitNewStmt(Stmt.New stmt)
        {
            _program.Clear();
            ResetVariables();
            return new Stmt.End();
        }

        Stmt Stmt.Visitor<Stmt>.VisitUserStmt(Stmt.User stmt)
        {
            
            var pattern = stmt.UserStatement.Tokens[stmt.Pattern];
            var literals = pattern.Where(x => x.Type == TokenType.Identifier);
            var expressions = stmt.Expressions;

            var inputVariables = new List<Expr.Variable>();
            var outputs = new List<object>();

            int i = 0;
            foreach (var l in literals)
            {
                if (l.Lexeme.StartsWith("I"))
                {
                    inputVariables.Add((Expr.Variable)expressions[i]);
                }
                else
                {
                    outputs.Add(Evaluate(expressions[i]));
                }
                i++;
            }

            var inputValues = inputVariables.Select(x => Evaluate(x)).ToArray();

            var result = stmt.UserStatement.Execute(this, stmt.Pattern, outputs.ToArray(), inputValues);

            for(int j=0; j<inputVariables.Count; j++) 
            {
                WriteVariable(inputVariables[j], inputValues[j]);
            }

            if (result is StatementResultError error)
                throw new RuntimeError(stmt.Name, error.Message);
            else if (result is StatementResultEnd)
                return new Stmt.End();

            return stmt.NextStatement;

        }
    }
}

