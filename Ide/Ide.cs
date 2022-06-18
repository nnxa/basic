using System;
using Basic.Ide.Functions;
using Basic.Ide.Statements;
using Basic.Interpreter;

namespace Basic.Ide
{
	public class Ide
	{

		private string _loadFile = "";

		public void Load(string fileName)
        {
			_loadFile = fileName;
        }

		

		public void RunPrompt()
        {

			var interpreter = new Interpreter.Interpreter();

			interpreter.AddPrintStatement(new Print());
			interpreter.AddStatement(new Color());
			interpreter.AddStatement(new Locate());

			interpreter.AddStatement(new Input());
			interpreter.AddStatement(new ListStmt());
			interpreter.AddStatement(new Load(this));
			interpreter.AddStatement(new Dir());
			interpreter.AddStatement(new Cls());
			interpreter.AddStatement(new Save());

			interpreter.AddFunction(new Inkey(), "INKEY%");

			Console.WriteLine("BASIC");
			Console.WriteLine();
			
			while (true)
            {
				Console.ResetColor();
				Console.Write("> ");
				var line = Console.ReadLine();
				if (line is not null)
                {
					var p = line.Trim();

					if (p.Equals("QUIT", StringComparison.InvariantCultureIgnoreCase))
						break;

					if (p.Equals("HELP", StringComparison.InvariantCultureIgnoreCase))
                    {
						
						var keywords = interpreter.Keywords().OrderBy(x => x).ToList();

						Console.WriteLine();
						Console.WriteLine("Valid keywords:");
						Console.WriteLine();

						Common.PrintInColumns(keywords);


						Console.WriteLine();

						continue;
                    }

					interpreter.Immediate(p);
					Console.ResetColor();

					if (_loadFile is not null && _loadFile != "")
                    {
						try
						{
							var fileLines = File.ReadAllLines(_loadFile + ".bas");
							interpreter.Immediate("NEW");
							foreach (var fileLine in fileLines)
							{
								interpreter.Immediate(fileLine);
							}
						}
						catch (Exception)
						{
							Console.Error.WriteLine("Could not load file.");
						}
						finally
						{
							_loadFile = "";
						}
                    }

				}
					
            }
        }

	}
}

