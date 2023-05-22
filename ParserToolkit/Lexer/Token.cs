using System;

// ReSharper disable UnusedMember.Global

namespace ParserToolkit.Lexer
{
    // The Enum constraint is available in C# 7.3
    public class Token<TToken> where TToken : Enum
    {
        public Token(TToken type, string value, int position, int line, int column)
        {
            Type = type;
            Value = value;
            Position = new Position(value, position, line, column);
        }

        public Position Position { get; }
        public TToken Type { get; }
        public string Value { get; }

    }
}
