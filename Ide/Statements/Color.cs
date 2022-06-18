using System;
using Basic.Interpreter;

namespace Basic.Ide.Statements
{
	public class Color : GenericUserStatement
	{
		public Color()
		{
		}

        public override string[] Patterns => new[] { "O", "O,O" };

        public override string Label => "COLOR";

        public override IStatementResult Execute(IContext context, int pattern, object[] parameters, object[] inputs)
        {
            try
            {

                var fg = Converters.ToInt(parameters[0]);
                Console.ForegroundColor = (ConsoleColor)fg;

                if (parameters.Length > 1)
                    Console.BackgroundColor = (ConsoleColor)Converters.ToInt(parameters[1]);

                return Ok();

            }
            catch (Exception e)
            {
                return Error(e.Message);
            }
        }
    }

    public class Locate : GenericUserStatement
    {
        
        public override string[] Patterns => new[] { "O,O" };

        public override string Label => "LOCATE";

        public override IStatementResult Execute(IContext context, int pattern, object[] parameters, object[] inputs)
        {
            try
            { 

                var y = ((int)(long)parameters[0]) - 1;
                var x = ((int)(long)parameters[1]) - 1;

                Console.CursorTop = y;
                Console.CursorLeft = x;

                return Ok();

            }
            catch (Exception e)
            {
                return Error(e.Message);
            }
        }
    }
}

