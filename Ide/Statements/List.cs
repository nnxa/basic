using System;
using Basic.Interpreter;

namespace Basic.Ide.Statements
{
    public class ListStmt : GenericUserStatement
    {
        public override string[] Patterns => new[] { "" };

        public override string Label => "LIST";

        public override IStatementResult Execute(IContext context, int pattern, object[] parameters, object[] inputs)
        {
            Console.Write(context.Listing());
            return Ok();
        }
    }
}

