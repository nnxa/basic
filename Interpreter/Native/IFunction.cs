using System;

namespace Basic.Interpreter.NativeFunctions
{
	public interface IFunction
	{
		bool AcceptsArity(long arity);
		object Call(object[] parameters);
	}
}

