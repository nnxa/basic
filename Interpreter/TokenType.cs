using System;

namespace Basic.Interpreter
{
	internal enum TokenType
    {

        // Single-character tokens
        LeftParen, RightParen, Colon, NewLine,
        Comma, Dot, Minus, Plus, Semicolon, Slash, Star,

        // One or two character tokens
        NotEqual, Equal, Greater, GreaterEqual, Less,
        LessEqual,

        // Literals
        Identifier, String, Number, DataLiteral,
        Comment,

        // Keywords
        And, Else, For, If, Or, Print, Return,
        Goto, Not, Gosub, Next, Let, Then, To,
        Step, Run, End, Xor, Dim, Read,
        Data, Rem, On, New, UserStatement,

        // EOF
        EOF,
        

    }
}

