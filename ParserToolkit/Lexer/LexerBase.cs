using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable UnusedMember.Global

namespace ParserToolkit
{
    public abstract class LexerBase<TToken> where TToken : Enum
    {
        private readonly string _input;
        private readonly List<Token<TToken>> _tokens = new List<Token<TToken>>();
        private readonly IList<string> _errors = new List<string>();

        // = 0
        protected int Position { get; set; }
        protected char Value { get; set; }
        protected int Line { get; set; } = 1;
        // = 0
        protected int Column { get; set; }

        protected LexerBase(string input)
        {
            _input = input;
        }

        // The only way to add tokens.
        protected abstract void ReadToken();

        public LexerResult<TToken> Tokenize()
        {
            while (!IsEndOfFile())
            {
                ReadToken();
            }

            return _errors.Any()
                ? new LexerResult<TToken> { Input = _input, Tokens = null, Errors = _errors }
                : new LexerResult<TToken> { Input = _input, Tokens = _tokens, Errors = null };
        }

        protected void AddToken(Token<TToken> token)
        {
            _tokens.Add(token);
        }

        protected void AddError(Token<TToken> currentToken, string expected, string message = "")
        {
            if (currentToken == null)
                throw new ArgumentNullException($"'{nameof(currentToken)}' argument is null.");


            if (string.IsNullOrEmpty(expected))
                throw new ArgumentNullException($"'{nameof(expected)}' argument is null or empty.");

            var error = Error(currentToken, expected, message);

            if (string.IsNullOrEmpty(error))
                throw new ArgumentNullException($"The 'Error()' function has returned null or empty.");

            _errors.Add(error);
        }

        protected virtual string Error(Token<TToken> currentToken, string expected, string message = "")
        {
            return
                $"Expecting '{expected}' but got '{currentToken.Value}' ({currentToken.Position.Line}:{currentToken.Position.Column})"
                + (string.IsNullOrEmpty(message) ? "" : Environment.NewLine + message);
        }

        protected bool IsMatch(char ch)
        {
            if (!IsEndOfFile())
            {
                return ch == Peek();
            }
            return false;
        }

        protected string ReadWhile(Func<char, bool> predicate)
        {
            var result = "";
            while (!IsEndOfFile() && predicate(Peek()))
            {
                var ch = Read();
                result += ch;
            }
            return result;
        }
        protected void SkipWhile(Func<char, bool> predicate)
        {
            ReadWhile(predicate);
        }

        protected string ReadLine(bool movePointerToNextLine = false)
        {
            var result = ReadWhile(ch => ch != '\n');
            if (result == null) return null;
            if (movePointerToNextLine) Skip();
            return result;
        }

        protected void SkipLine(bool movePointerToNextLine = false)
        {
            if (movePointerToNextLine)
                ReadLine(true);
            else
                ReadLine();
        }

        protected string ReadEscaped(char ch, char indicator)
        {
            var escaped = false;
            var result = "";
            Skip(); // The indicator itself.
            while (!IsEndOfFile())
            {
                var c = Read();

                if (escaped)
                {
                    result += c;
                    escaped = false;
                }
                else if (c == indicator)
                {
                    escaped = true;
                }
                else if (c == ch)
                {
                    break;
                }
                else
                {
                    result += c;
                }
            }
            return result;
        }

        protected virtual string ReadNumber(out bool isFloat)
        {
            var hasDot = false;
            var result = ReadWhile(ch =>
             {
                 if (ch != '.') return char.IsDigit(ch);
                 if (hasDot) return false;
                 hasDot = true;
                 return true;
             });
            isFloat = hasDot;
            return result;
        }


        protected char Peek()
        {
            if (Position < _input.Length)
            {
                return _input[Position];
            }
            else
            {
                return '\0';
            }
        }

        protected char[] Peek(int lookahead)
        {
            if (lookahead < 1)
            {
                throw new Exception($"'{nameof(lookahead)}' should be greater than 0.");
            }
            var list = new List<char>();
            for (int i = 0; i < lookahead; i++)
            {
                var pos = Position + i;
                if (pos < _input.Length)
                {
                    list.Add(_input[pos]);
                }
                else
                {
                    list.Add('\0');
                    break;
                }
            }
            return list.ToArray();
        }

        // Next
        // Advance
        // Consume
        protected char Read()
        {
            var value = Peek();
            Position++;
            Value = value;
            if (value == '\0')
                return '\0';

            if (value == '\n')
            {
                Line++;
                Column = 0;
            }
            else
            {
                Column++;
            }
            return value;
        }

        protected char[] Read(int lookahead)
        {
            if (lookahead < 1)
            {
                throw new Exception($"'{nameof(lookahead)}' should be greater than 0.");
            }
            var list = new List<char>();
            for (int i = 0; i < lookahead; i++)
            {
                var value = Peek();
                Position++;
                Value = value;
                list.Add(value);

                if (value == '\0')
                {
                    break;
                }

                if (value == '\n')
                {
                    Line++;
                    Column = 0;
                }
                else
                {
                    Column++;
                }
            }
            return list.ToArray();
        }

        // EOF
        protected bool IsEndOfFile()
        {
            return Position >= _input.Length || Value == '\0';
        }

        protected string PeekAsString()
        {
            return Peek().ToString();
        }

        protected string PeekAsString(int lookahead)
        {
            return new string(Peek(lookahead));
        }

        protected string ReadAsString()
        {
            return Read().ToString();
        }

        protected string ReadAsString(int lookahead)
        {
            return new string(Read(lookahead));
        }

        protected void Skip()
        {
            Read();
        }

        protected virtual bool IsIdentifierCandidate(char ch, Regex regex = null)
        {
            var re = regex ?? new Regex("^[_a-zA-Z0-9]+$", RegexOptions.Compiled);
            return ch.ToString().IsRegexMatch(re);
        }

        protected virtual bool IsOperator(string value)
        {
            var operators = new[]
            {
                "+", "-", "/", "*", "%", "<", ">", ">=", "<=", "!=", "=","==","===", "++", "--", "&", "^", "|", ">>", "<<", "&&",
                "||", "??","+=","-=","*=","/=","%=","|=","&=","^=","<<=",">>=","??=","=>","..","~","?.","->"
            };

            return operators.Contains(value);
        }
    }
}
