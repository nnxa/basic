using System;
namespace Basic.Ide
{
	public static class Common
	{

		public static void PrintInColumns(IReadOnlyList<string> fields)
		{
			var fieldWidth = fields.Max(x => x.Length) + 2;
			var cols = Console.WindowWidth / fieldWidth;

			int i = 0;

			foreach (var k in fields)
			{
				Console.Write(k);
				i++;
				if (i % cols == 0)
				{
					Console.WriteLine();
				}
				else
				{
					Console.Write(new string(' ', fieldWidth - k.Length));
				}
			}

			if (i % cols != 0) Console.WriteLine();
		}
	}
}

