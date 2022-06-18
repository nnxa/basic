using System;
using Basic.Interpreter;

namespace Basic.Ide.Statements
{
    public class Input : GenericUserStatement
    {
        public override string[] Patterns => new[]
        {
            "I",
            "O;I",
            "O,I"
        };

        public override string Label => "INPUT";

        public override IStatementResult Execute(IContext context, int pattern, object[] parameters, object[] inputs)
        {
            if (pattern == 0)
            {
                Console.Write("] ");
            }    
            else if (pattern == 1)
            {
                Console.Write(Convert.ToString(parameters[0]) + "] ");
            }
            else if (pattern == 2)
            {
                Console.Write(Convert.ToString(parameters[0]));
            }


            var variable = inputs[0];
            while (true)
            {
                var input = Console.ReadLine();
                if (variable is long && long.TryParse(input, out var newInt))
                {
                    inputs[0] = newInt;
                    break;
                }
                else if (variable is double && double.TryParse(input, out var newDouble))
                {
                    inputs[0] = newDouble;
                    break;
                }
                else if (variable is string)
                {
                    inputs[0] = input ?? "";
                    break;
                }
            }

            return Ok();
        }
    }
}

