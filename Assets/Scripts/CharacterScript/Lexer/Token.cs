using System;
using System.Collections.Generic;

namespace VD.Rulebound.CS
{
    public class Token
    {
        internal TokenType Type;
        internal int Line;
        internal string SourceName;
        internal object Value;

        internal Token(TokenType type, int line, string sourceName)
        {
            Type = type;
            Line = line;
            SourceName = sourceName;
            Value = null;
        }

        internal Token(TokenType type, int line, string sourceName, object value) : this(type, line, sourceName)
        {
            Value = value;
        }
    }

    internal enum TokenType
    {
        OPEN_BRACE, CLOSE_BRACE, OPEN_SQUARE, CLOSE_SQUARE,
        COMMA,
        RARROW,

        NUM, STR, ID,

        DIALOGUE,
        GIVE, TAKE, ITEM,
        IF, FLAG, GLOBAL, SET, UNSET, NOT,
        WAIT, PORTRAIT, CHOICES,

        EOF,
    }
}
