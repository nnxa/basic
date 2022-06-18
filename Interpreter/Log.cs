using System;

namespace Basic.Interpreter
{

	public enum ErrorType
	{
		Syntax, Block
	}

	public class ErrorMessage
	{

		public ErrorType ErrorType { get; }
		public string Message { get; }

		public ErrorMessage(ErrorType errorType, string message)
		{
			ErrorType = errorType;
			Message = message;
		}

	}

	public class Log
	{

		private List<ErrorMessage> _messages = new List<ErrorMessage>();

		internal bool Any(Predicate<ErrorMessage> predicate)
        {
			return _messages.Any(x => predicate(x));
        }

		internal bool Any()
		{
			return _messages.Count > 0;
        }

		internal List<ErrorMessage> PopMessages()
        {
			var m = _messages;
			_messages = new List<ErrorMessage>();
			return m;
        }

		internal void Error(ErrorType errorType, Token token, string message)
        {
			if (token.Type == TokenType.EOF)
            {
				Report(errorType, token.Line, " at end", message);
            }
			else
            {
				Report(errorType, token.Line, " at '" + token.Lexeme + "'", message);
            }
        }

		internal void Error(ErrorType errorType, long line, string message)
        {
			Report(errorType, line, "", message);
        }

		internal void Report(ErrorType errorType, long line, string where, string message)
        {
			var prefix = line > 0 ? $"[line {line}] " : "";
			_messages.Add(new ErrorMessage(errorType, $"{prefix}Error{where}: {message}"));
        }

	}
}

