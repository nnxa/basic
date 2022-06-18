using System;

namespace Basic.Interpreter
{
	public interface IStatementResult { }

	public interface IContext
    {
		string Listing();
    }

	public abstract class UserStatement
	{

		internal abstract TokenType TokenType { get; }
		public abstract string Label { get; }

		internal class StatementResultEnd : IStatementResult { }

		internal class StatementResultOK : IStatementResult
		{
			internal StatementResultOK() { }
		}

		internal class StatementResultError : IStatementResult
		{

			public string Message { get; }

			internal StatementResultError(string message)
			{
				Message = message;
			}

		}

		public IStatementResult Ok()
		{
			return new StatementResultOK();
		}

		public IStatementResult Error(string message)
		{
			return new StatementResultError(message);
		}

		public IStatementResult End()
        {
			return new StatementResultEnd();
        }

	}

	public abstract class GenericUserStatement : UserStatement
    {

		public abstract IStatementResult Execute(IContext context, int pattern, object[] parameters, object[] inputs); 

		public abstract string[] Patterns { get; }

		internal override TokenType TokenType => TokenType.UserStatement;

        public GenericUserStatement()
        {
			_tokenGenerator = new Lazy<Token[][]>(() =>
				Patterns.Select(pattern =>
				{

					var log = new Log();
					var scanner = new Scanner(log, pattern);

					if (log.Any())
						throw new Exception($"Error in pattern '{pattern}'\n" + string.Join(System.Environment.NewLine, log.PopMessages()));

					return scanner.Tokens().ToArray();
				}).ToArray());
		}

		private readonly Lazy<Token[][]> _tokenGenerator;


		internal Token[][] Tokens => _tokenGenerator.Value;

	}

	public abstract class PrintUserStatement : UserStatement
	{
		internal override TokenType TokenType => TokenType.Print;
        public abstract IStatementResult Execute(string content);
	}

}

