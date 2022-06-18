using Basic.Interpreter;

namespace Basic.Ide.Statements
{

	public class Cls : GenericUserStatement
	{

        public override string Label => "CLS";

        public override string[] Patterns => new[] { "" }; 

        public override IStatementResult Execute(IContext context, int pattern, object[] parameters, object[] inputs)
        {
            Console.Clear();
            return Ok();
        }

    }

}

