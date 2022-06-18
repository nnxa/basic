using Basic.Interpreter;

namespace Basic.Ide.Statements
{

	public class Print : PrintUserStatement
	{

		public override string Label => "PRINT";

        public override IStatementResult Execute(string content)
        {
			Console.Write(content);
			return Ok();
		}

	}

}

