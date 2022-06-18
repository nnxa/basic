using System;
using Basic.Interpreter.NativeFunctions;

namespace Basic.Ide.Functions
{
    public class Inkey : IFunction
    {

        public bool AcceptsArity(long arity)
        {
            return arity == 0;
        }

        public object Call(object[] parameters)
        {
            if (Console.KeyAvailable)
                return (long)Console.ReadKey(true).Key;
            else
                return 0L;
        }
    }
}

