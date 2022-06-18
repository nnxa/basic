using System;
using Basic.Interpreter;

namespace Basic.Ide.Statements
{
    public class Dir : GenericUserStatement
    {
        
        public override string[] Patterns => new[] { "" };

        public override string Label => "DIR";

        public override IStatementResult Execute(IContext context, int pattern, object[] parameters, object[] inputs)
        {
            try
            {
                Common.PrintInColumns(Directory.EnumerateFiles(".", "*.bas")
                    .Select(x => x.ToUpperInvariant()
                                  .Replace(".BAS", "")
                                  .Replace("./", ""))
                    .OrderBy(x => x).ToList());
                return Ok();
            }
            catch
            {
                return Error("Could not access directory.");
            }
        }

    }
}

