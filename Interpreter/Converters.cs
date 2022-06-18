using System;
namespace Basic.Interpreter
{
	public static class Converters
	{
		public static long ToInt(object value)
        {
			if (value is long i) return i;
			else if (value is double d) return (long)d;
			else throw new TokenlessRuntimeError("Type mismatch.");
		}

		public static string ToString(object value)
        {
			if (value is string s) return s;
			throw new TokenlessRuntimeError("Type mismatch.");
		}

        internal static object ToDouble(object value)
        {
			if (value is long i) return (double)i;
			else if (value is double d) return d;
			else throw new TokenlessRuntimeError("Type mismatch.");
		}
    }
}

