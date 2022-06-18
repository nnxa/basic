using System;

namespace Basic.Interpreter
{

	// Tokenizes the raw code, e.g.
	/// '10 PRINT "HELLO, WORLD."' becomes [10] [PRINT] [HELLO, WORLD.] [EOF]
	internal class Scanner
	{
			
        private readonly string _source;
        private readonly List<Token> _tokens = new List<Token>();
		private readonly Log _log;

		private readonly Dictionary<string, TokenType> _keywords;

		private int _start = 0;
		private int _current = 0;
		private int _line = 1;

		public Scanner(Log log, string source)
			: this (log, source, Enumerable.Empty<KeyValuePair<string, TokenType>>())
        {

        }

		public Scanner(Log log,
			           string source,
					   IEnumerable<KeyValuePair<string, TokenType>> customKeywords)
		{

			_log = log;
			_source = source;

			_keywords = new Dictionary<string, TokenType>(StringComparer.InvariantCultureIgnoreCase)
			{
				{ "AND", TokenType.And },
				{ "DATA", TokenType.Data },
				{ "DIM", TokenType.Dim },
				{ "ELSE", TokenType.Else },
				{ "END", TokenType.End },
				{ "FOR", TokenType.For },
				{ "GOSUB", TokenType.Gosub },
				{ "GOTO", TokenType.Goto },
				{ "IF", TokenType.If },
				{ "LET", TokenType.Let },
				{ "ON", TokenType.On },
				{ "OR", TokenType.Or },
				{ "NEW", TokenType.New },
				{ "NEXT", TokenType.Next },
				{ "NOT", TokenType.Not },
				{ "READ", TokenType.Read },
				{ "REM", TokenType.Rem },
				{ "RETURN", TokenType.Return },
				{ "RUN", TokenType.Run },
				{ "STEP", TokenType.Step },
				{ "THEN", TokenType.Then },
				{ "TO", TokenType.To },
				{ "XOR", TokenType.Xor }
			};

			foreach (var s in customKeywords)
            {
				_keywords.Add(s.Key.ToUpperInvariant(), s.Value);
            }

		}

		public IEnumerable<string> Keywords => _keywords.Keys;

		public List<Token> Tokens()
        {
			while (!IsAtEnd())
            {
				_start = _current;
				ScanToken();
            }

			_tokens.Add(new Token(TokenType.EOF, "", 0.0, _line));
			return _tokens;
        }

		private void ScanToken()
        {
			var c = Advance();
			switch (c)
            {

				case '(': AddToken(TokenType.LeftParen); break;
				case ')': AddToken(TokenType.RightParen); break;
				case ',': AddToken(TokenType.Comma); break;
				case '.': AddToken(TokenType.Dot); break;
				case '-': AddToken(TokenType.Minus); break;
				case '+': AddToken(TokenType.Plus); break;
				case ';': AddToken(TokenType.Semicolon); break;
				case ':': AddToken(TokenType.Colon); break;
				case '*': AddToken(TokenType.Star); break;
				case '/': AddToken(TokenType.Slash); break;
				case '=': AddToken(TokenType.Equal); break;

				case '<':
					if (Match('='))
						AddToken(TokenType.LessEqual);
					else if (Match('>'))
						AddToken(TokenType.NotEqual);
					else
						AddToken(TokenType.Less);
					break;
				case '>':
					if (Match('='))
						AddToken(TokenType.GreaterEqual);
					else
						AddToken(TokenType.Greater);
					break;

				case '"': ScanString(TokenType.String); break;

				// Comments 
				case '\'':
					{
						while (Peek() != '\n' && !IsAtEnd()) Advance();
					}
					break;

				case ' ':
				case '\t':
				case '\r':
					// Ignore whitespace;
					break;

				case '\n':
					AddToken(TokenType.NewLine);
					_line++;
					break;

				default:
					if (IsDigit(c))
					{
						Number();
					}
					else if (IsAlpha(c))
                    {
						Identifier();
                    }
					else
					{
						_log.Error(ErrorType.Syntax, _line, "Unexpected character.");
					}
					break;
            }
        }

		private bool Match(char expected)
        {
			if (IsAtEnd()) return false;
			if (_source[_current] != expected) return false;
			_current++;
			return true;
        }

		private char Advance()
        {
			return _source[_current++];
        }

		private char Peek()
        {
			if (IsAtEnd()) return '\0';
			return _source[_current];
        }

		private char PeekNext()
        {
			if (_current + 1 >= _source.Length) return '\0';
			return _source[_current + 1];
        }

		private bool IsDigit(char c)
        {
			return c >= '0' && c <= '9';
        }

		private bool IsAlpha(char c)
        {
			return (c >= 'a' && c <= 'z')
				|| (c >= 'A' && c <= 'Z')
				|| (c == '_');
        }

		private bool IsAlphaNumeric(char c)
        {
			return IsDigit(c) || IsAlpha(c);
        }

		private bool IsAtEnd()
		{
			return _current >= _source.Length;
		}

		private void ScanString(TokenType tokenType)
        {

			while (Peek() != '"' && !IsAtEnd() && Peek() != '\n')
            {
				Advance();
            }

			if (Peek() != '"')
            {
				_log.Error(ErrorType.Syntax, _line, "Unterminated string");
				return;
            }

			Advance(); // Consume closing ".

			var value = _source.Substring(_start + 1, _current - _start - 2);
			AddToken(tokenType, value);

        }

		private void ScanUnquotedData()
		{

			
			while ( Peek() != ',' && !IsAtEnd() && Peek() != '\n')
			{
				Advance();
			}

			var value = _source.Substring(_start, _current - _start).Trim();
			AddToken(TokenType.DataLiteral, value);

		}

		private void Number()
        {

			
			while (IsDigit(Peek())) Advance();

			// Look for fractional part
			if (Peek() == '.' && IsDigit(PeekNext()))
			{
				// Consume the "."
				Advance();

				while (IsDigit(Peek())) Advance();

				AddToken(TokenType.Number,
				double.Parse(_source.Substring(_start, _current - _start)));

			}
			else
			{
				AddToken(TokenType.Number,
					long.Parse(_source.Substring(_start, _current - _start)));
			}
		
        }

		private bool EndOfLine()
        {
			return IsAtEnd() || Peek() == '\r' || Peek() == '\n';

		}

		private void Identifier()
		{
			while (IsAlphaNumeric(Peek())) Advance();

			if (Peek() == '%' || Peek() == '$') Advance();

			var text = _source.Substring(_start, _current - _start);
			if (_keywords.TryGetValue(text, out var type))
			{

				AddToken(type);

				if (type == TokenType.Rem)
				{

					ConsumeNonBreakingWhitespace();
					_start = _current;
					while (!EndOfLine())
                    {
						Advance();
                    }
					AddToken(TokenType.Comment, _source.Substring(_start, _current - _start));

				}

				if (type == TokenType.Data)
                {

					while (!EndOfLine())
					{

						ConsumeNonBreakingWhitespace();

						if (Peek() == '"')
						{
							_start = _current;
							Advance();
							ScanString(TokenType.DataLiteral);
							if (Peek() == ',')
							{
								_start = _current;
								Advance();
								AddToken(TokenType.Comma);
								
							}
							if (Peek() == ':') break;
						}
						else
						{ 
							_start = _current;
							ScanUnquotedData();
							if (Peek() == ',')
							{
								_start = _current;
								Advance();
								AddToken(TokenType.Comma);
							}
						}
					}
                }
			}
			else
			{
				AddToken(TokenType.Identifier);
			}
		}

		private void ConsumeNonBreakingWhitespace()
        {
			while (Peek() == ' ' || Peek() == '\t') Advance();
		}

		private void AddToken(TokenType type, object? literal = null)
        {
			var text = _source.Substring(_start, _current - _start);
			if (type != TokenType.String) text = text.ToUpperInvariant();
			_tokens.Add(new Token(type, text, literal ?? 0.0, _line));
        }

		
	}
}

