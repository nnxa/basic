using System;
using Basic.Interpreter.NativeFunctions;

namespace Basic.Interpreter
{

	internal class ResolvedVariable
    {
		public Token Token { get; }
		public long[] Element { get; }

		public ResolvedVariable(Token token, IEnumerable<long> element)
        {
			Token = token;
			Element = element.ToArray();
        }
    }

	internal class Environment
	{

		private readonly Dictionary<string, object> _values = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

		public void Define(string name, long[] bounds)
        {
			var defaultValue = Default(name);
			if (bounds.Length == 0)
			{
				Define(name, defaultValue);
			}
			else
			{
				if (_values.ContainsKey(name))
					throw new TokenlessRuntimeError("Variable already defined.");

				var newArray = Array.CreateInstance(defaultValue.GetType(), bounds.Select(x => x + 1).ToArray());
				_values.Add(name, newArray);
			}
        }

		public void Define(string name, object value)
        {
			Assign(name, value);
        }

		private Array GetArray(Token token)
        {
			var name = token.Lexeme;
			if (!_values.TryGetValue(name, out var array))
				throw new RuntimeError(token, "Undefined array.");
			if (!(array is Array arr))
				throw new RuntimeError(token, "Variable is not an array.");
			return arr;
		}

		public void Assign(ResolvedVariable variable, object value)
        {
			Assign(variable.Token, variable.Element, value);
        }

		private void Assign(Token variable, long[] element, object value)
        {
			
			if (element.Length == 0)
			{
				Assign(variable, value);
			}
			else
			{

				var arr = GetArray(variable);

				try
				{
					var ex = arr.GetValue(element) ?? "";
					arr.SetValue(Cast(value, ex), element);
				}
				catch (IndexOutOfRangeException)
                {
					throw new RuntimeError(variable, "Subscript out of range.");
                }
				catch (TokenlessRuntimeError e)
                {
					throw new RuntimeError(variable, e.Message);
                }

			}
        }

		private void Assign(Token variable, object value)
        {
			try
			{
				Assign(variable.Lexeme, value);
			}
			catch (TokenlessRuntimeError e)
            {
				throw new RuntimeError(variable, e.Message);
            }
        }

		private void Assign(string name, object value)
		{

			if (!_values.TryGetValue(name, out var exValue))
            {
				exValue = Default(name, value);
            }

			_values[name] = Cast(value, exValue);

        }

		private object Cast(object from, object to)
        {
			if (to is long)
			{
				return Converters.ToInt(from);
			}
			else if (to is double)
			{
				return Converters.ToDouble(from);
			}
			else if (to is string) return (string)from;
			else if (to is IFunction) return from;
			else if (to.GetType().IsArray) throw new TokenlessRuntimeError("Cannot assign to array.");
			else throw new TokenlessRuntimeError("Type mismatch.");

		}

		private object Default(string name, object value)
        {
			if (value is IFunction) return value;
			return Default(name);
        }

		private object Default(string name)
        {
			var qualifier = name[name.Length - 1];
			if (qualifier == '%') return (long)0;
			if (qualifier == '$') return (string)"";
			return (double)0.0;
		}

		public bool IsArray(Token name)
        {
			if (!_values.TryGetValue(name.Lexeme, out var result))
				return false;
			return result.GetType().IsArray;
        }

		public bool IsFunction(Token name, out IFunction? fn)
		{
			if (_values.TryGetValue(name.Lexeme, out var result) && result is IFunction q)
            {
				fn = q;
				return true;
            }
			fn = null;
			return false;
		}

		private object Get(Token name)
        {
			if (!_values.TryGetValue(name.Lexeme, out var result))
            {
				var d = Default(name.Lexeme);
				_values[name.Lexeme] = d;
				return d;
            }

			if (result.GetType().IsArray)
				throw new RuntimeError(name, "Variable is array.");

			return result;
        }

		public object Get(ResolvedVariable variable)
        {
			var name = variable.Token;
			var element = variable.Element;

			if (element.Length == 0) return Get(name);

			var array = GetArray(name);
			object? value;
			try
			{
				value = array.GetValue(element) ?? "";
			}
			catch (TokenlessRuntimeError e)
            {
				throw new RuntimeError(name, e.Message);
            }
			return value;

        }
	}
}

