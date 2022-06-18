using System;

namespace Basic.Interpreter.NativeFunctions
{
	public class Abs : IFunction
	{
		public Abs()
		{
		}

        public bool AcceptsArity(long arity)
        {
            return arity == 1;
        }

        public object Call(object[] parameters)
        {
            var value = parameters[0];
            if (value is long i) return Math.Abs(i);
            if (value is double d) return Math.Abs(d);
            throw new TokenlessRuntimeError("Type mismatch.");
        }
    }
}

