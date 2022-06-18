using System;
using Basic.Interpreter;

namespace Basic.Ide.Statements
{
    public class Save : GenericUserStatement
    {

        public override string[] Patterns => new[] { "O" };

        public override string Label => "SAVE";

        public override IStatementResult Execute(IContext context, int pattern, object[] parameters, object[] inputs)
        {
            try
            {
                var fileName = Converters.ToString(parameters[0]);
                File.WriteAllText(fileName + ".bas", context.Listing());
                return Ok();
            }
            catch
            {
                return Error("Could not write file.");
            }
        }
    }
}

