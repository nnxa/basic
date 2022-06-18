using System;
using Basic.Interpreter;

namespace Basic.Ide.Statements
{
    public class Load : GenericUserStatement
    {
        private readonly Ide _ide;

        public Load(Ide ide)
        {
            _ide = ide;
        }

        public override string[] Patterns => new[] { "O" };

        public override string Label => "LOAD";

        public override IStatementResult Execute(IContext context, int pattern, object[] parameters, object[] inputs)
        {
            string fileName;
            try
            {
                fileName = Converters.ToString(parameters[0]);
            }
            catch (Exception e)
            {
                return Error(e.Message);
            }

            _ide.Load(fileName);
            return End();
            
        }
    }
}

