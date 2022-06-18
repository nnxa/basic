namespace Basic.Interpreter.NativeFunctions
{
    public class Len : IFunction
    {
        public Len()
        {
        }

        public bool AcceptsArity(long arity)
        {
            return arity == 1;
        }

        public object Call(object[] parameters)
        {
            var s = Converters.ToString(parameters[0]);
            return (long)s.Length;
        }
    }

    public class Asc : IFunction
    {
        public Asc()
        {
        }

        public bool AcceptsArity(long arity)
        {
            return arity == 1;
        }

        public object Call(object[] parameters)
        {
            var s = Converters.ToString(parameters[0]);
            if (s.Length != 1) throw new TokenlessRuntimeError("Expected string of length 1.");
            return (long)s[0];
        }
    }

    public class Chr : IFunction
    {
        public Chr()
        {
        }

        public bool AcceptsArity(long arity)
        {
            return arity == 1;
        }

        public object Call(object[] parameters)
        {
            var asciiCode = Converters.ToInt(parameters[0]);
            return new string((char)asciiCode, 1);
        }
    }

    public class Left : IFunction
    {
        public Left()
        {
        }

        public bool AcceptsArity(long arity)
        {
            return arity == 2;
        }

        public object Call(object[] parameters)
        {
            var inputString = Converters.ToString(parameters[0]);
            var chars = (int)Converters.ToInt(parameters[1]);
            if (chars >= inputString.Length) return inputString;
            return inputString.Substring(0, chars);
        }
    }

    public class Right : IFunction
    {
        public Right()
        {
        }

        public bool AcceptsArity(long arity)
        {
            return arity == 2;
        }

        public object Call(object[] parameters)
        {
            var inputString = Converters.ToString(parameters[0]);
            var chars = (int)Converters.ToInt(parameters[1]);
            if (chars >= inputString.Length) return inputString;
            return inputString.Substring(inputString.Length - chars, chars);
        }
    }

    public class Mid : IFunction
    {
        public bool AcceptsArity(long arity)
        {
            return arity == 2 || arity == 3;
        }

        public object Call(object[] parameters)
        {
            var inputString = Converters.ToString(parameters[0]);
            var startIndex = (int)Converters.ToInt(parameters[1]) - 1;

            if (parameters.Length == 3)
            {
                var length = (int)Converters.ToInt(parameters[2]);
                return inputString.Substring(startIndex, length);
            }
            return inputString.Substring(startIndex);
        }
    }
}

