using System;

namespace Basic.Interpreter.NativeFunctions
{
	public class Int : IFunction
	{

        public bool AcceptsArity(long arity)
        {
            return arity == 1;
        }

        public object Call(object[] parameters)
        {
            return Converters.ToInt(parameters[0]);
        }
    }
}

