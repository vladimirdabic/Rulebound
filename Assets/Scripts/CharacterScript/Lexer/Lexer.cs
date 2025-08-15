using System;
using System.Collections.Generic;

namespace VD.Rulebound.CS
{
    internal class Lexer
    {
        private List<Token> _tokens;

        private int _current;
        private int _start;
        private int _line;

        private string _source;
        private string _ctxName;

        private static readonly Dictionary<string, TokenType> _reservedWords = new Dictionary<string, TokenType>()
        {
            {"dialogue", TokenType.DIALOGUE},
            {"give", TokenType.GIVE},
            {"take", TokenType.TAKE},
            {"item", TokenType.ITEM},
            {"if", TokenType.IF},
            {"flag", TokenType.FLAG},
            {"global", TokenType.GLOBAL},
            {"set", TokenType.SET},
            {"unset", TokenType.UNSET},
            {"wait", TokenType.WAIT},
            {"not", TokenType.NOT},
            {"portrait", TokenType.PORTRAIT},
            {"choices", TokenType.CHOICES},
        };

        public Token[] Scan(string code, string ctxName = "unknown")
        {
            _tokens = new List<Token>();
            _current = 0;
            _line = 1;
            _ctxName = ctxName;
            _source = code;

            while (Available())
            {
                _start = _current;
                Token tok = ScanToken();
                if(tok != null) _tokens.Add(tok);
            }

            _tokens.Add(GetToken(TokenType.EOF));
            return _tokens.ToArray();
        }

        private Token ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                case '{': return GetToken(TokenType.OPEN_BRACE);
                case '}': return GetToken(TokenType.CLOSE_BRACE);
                case '[': return GetToken(TokenType.OPEN_SQUARE);
                case ']': return GetToken(TokenType.CLOSE_SQUARE);
                case ',': return GetToken(TokenType.COMMA);
                case '-':
                    if (Match('>'))
                        return GetToken(TokenType.RARROW);
                    else
                        throw Error($"Expected character '>' after '-'");

                case '"':
                    return ScanStr();

                case '/':
                    if (Match('/'))
                    {
                        while (Available())
                            if (Match('\n'))
                            {
                                _line++;
                                break;
                            }
                            else
                                _current++;

                        return null;
                    }
                    else
                        throw Error("Unexpected character '/'");

                case '\n':
                    _line++;
                    return null;

                case ' ':
                case '\t':
                case '\r':
                    return null;

                default:
                    if (char.IsLetter(c) || c == '_')
                        return ScanId();
                    else if (char.IsDigit(c))
                        return ScanNum();

                    throw Error($"Unexpected character '{c}'");

            }
        }

        private Token ScanId()
        {
            while (Available() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
                Advance();

            string id = _source.Substring(_start, _current - _start);
            TokenType type = _reservedWords.ContainsKey(id) ? _reservedWords[id] : TokenType.ID;
            return GetToken(type, id);
        }

        private Token ScanNum()
        {
            while (Available() && char.IsDigit(Peek()))
                Advance();

            // decimal point
            if(Match('.'))
                while (Available() && char.IsDigit(Peek()))
                    Advance();

            string num = _source.Substring(_start, _current - _start);
            float value = float.Parse(num);

            return GetToken(TokenType.NUM, value);
        }

        private Token ScanStr()
        {
            string str = string.Empty;

            while(Available())
            {
                char c = Advance();

                if (c == '"')
                    break;
                else if (c == '\\')
                {
                    char escaped = Advance();

                    c = escaped switch
                    {
                        'n' => '\n',
                        _ => escaped,
                    };
                }

                str += c;
            }

            return GetToken(TokenType.STR, str);
        }

        private char Advance()
        {
            return _source[_current++];
        }

        private bool Available()
        {
            return _current < _source.Length;
        }

        private char Peek()
        {
            return Available() ? _source[_current] : '\0';
        }

        private bool Match(char c)
        {
            if(Peek() == c)
            {
                Advance();
                return true;
            }

            return false;
        }

        private LexerException Error(string msg)
        {
            return new LexerException(msg, _line, _ctxName);
        }

        private Token GetToken(TokenType type, object value = null)
        {
            return new Token(type, _line, _ctxName, value);
        }
    }

    public class LexerException : Exception
    {
        public int Line;
        public string Context;

        public LexerException(string message, int line, string ctx) : base(message)
        {
            Line = line;
            Context = ctx;
        }
    }
}
