﻿using System;

namespace Basic.Interpreter
{

	internal class TokenlessRuntimeError : Exception
    {
		public TokenlessRuntimeError(string message) : base(message) { }
    }

	internal class RuntimeError : Exception
	{

		public Token Token { get; }

		public RuntimeError(Token token, string message) : base(message)
		{
			Token = token;
		}
	}
}

