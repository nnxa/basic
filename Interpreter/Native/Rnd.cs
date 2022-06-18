namespace Basic.Interpreter.NativeFunctions
{
	public class Rnd : IFunction
	{

        private readonly Random _rnd = new Random();

        public bool AcceptsArity(long arity)
        {
            return arity == 0;
        }

        public object Call(object[] parameters)
        {
            return _rnd.NextDouble();
        }
    }
}

