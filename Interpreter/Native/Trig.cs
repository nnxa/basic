namespace Basic.Interpreter.NativeFunctions
{

    public abstract class MathFn : IFunction
    {

        public abstract double IntFn(long value);
        public abstract double DblFn(double value);

        public bool AcceptsArity(long arity)
        {
            return arity == 1;
        }

        public object Call(object[] parameters)
        {
            var value = parameters[0];
            if (value is double dbl) return DblFn(dbl);
            if (value is long i) return IntFn(i);
            throw new TokenlessRuntimeError("Type mismatch.");
        }
    }

    public class Sqr : MathFn
    {
        public override double DblFn(double value) => Math.Sqrt(value);
        public override double IntFn(long value) => Math.Sqrt(value);
    }

    public class Sin : MathFn
    {
        public override double DblFn(double value) => Math.Sin(value);
        public override double IntFn(long value) => Math.Sin(value);
    }

    public class Cos : MathFn
    {
        public override double DblFn(double value) => Math.Cos(value);
        public override double IntFn(long value) => Math.Cos(value);
    }

    public class Tan : MathFn
    {
        public override double DblFn(double value) => Math.Tan(value);
        public override double IntFn(long value) => Math.Tan(value);
    }

    public class Asin : MathFn
    {
        public override double DblFn(double value) => Math.Asin(value);
        public override double IntFn(long value) => Math.Asin(value);
    }

    public class Acos : MathFn
    {
        public override double DblFn(double value) => Math.Acos(value);
        public override double IntFn(long value) => Math.Acos(value);
    }

    public class Atan : MathFn
    {
        public override double DblFn(double value) => Math.Atan(value);
        public override double IntFn(long value) => Math.Atan(value);
    }

    

}

