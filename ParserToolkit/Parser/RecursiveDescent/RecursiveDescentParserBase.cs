using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable NotResolvedInText
// ReSharper disable CollectionNeverQueried.Local
// ReSharper disable UnusedMember.Global

namespace ParserToolkit.RecursiveDescent
{
    public abstract class RecursiveDescentParserBase<TToken, TResult>
        where TToken : Enum
        where TResult : class, new()
    {
        private readonly IEnumerator<Token<TToken>> _tokens;
        private readonly IList<string> _errors = new List<string>();

        protected RecursiveDescentParserBase(LexerResult<TToken> lexerResult)
        {
            if (lexerResult.Tokens == null || !lexerResult.Tokens.Any())
            {
                throw new Exception("The lexer result has no token.");
            }

            // Iterates through our tokens (collection).
            _tokens = lexerResult.Tokens.GetEnumerator();

            // To Start tokens processing, now 'Peek()' has a value and IEnumerator's Current is not null.
            Read();
        }

        protected abstract TResult Process();

        public ParserResult<TResult> Parse()
        {
            var result = Process();
            if (_errors.Any())
            {
                return new ParserResult<TResult>
                {
                    Errors = _errors,
                    Result = null
                };
            }

            return new ParserResult<TResult>
            {
                Errors = null,
                Result = result
            };

        }

        protected void AddError(Token<TToken> currentToken, string expected, string message = "")
        {
            if (currentToken == null)
                throw new ArgumentNullException($"'{nameof(currentToken)}' argument is null.");


            if (string.IsNullOrEmpty(expected))
                throw new ArgumentNullException($"'{nameof(expected)}' argument is null or empty.");

            var error = Error(currentToken, expected, message);

            if (string.IsNullOrEmpty(error))
                throw new ArgumentNullException("The 'Error()' function has returned null or empty.");

            _errors.Add(error);
        }

        protected bool Read()
        {
            return _tokens.MoveNext();
        }

        protected Token<TToken> Peek()
        {
            return _tokens.Current;
        }

        protected bool IsEndOfInput()
        {
            return _tokens.Current == null;
        }

        // More functionality
        protected void Skip()
        {
            Read();
        }

        protected void Read<T>(Func<T> func)
        {
            var status = Read();
            if (!status) func();
        }

        protected virtual string Error(Token<TToken> currentToken, string expected, string message = "")
        {
            return
                    $"Expecting '{expected}' but got '{currentToken.Value}' ({currentToken.Position.Line}:{currentToken.Position.Column})"
                    + (string.IsNullOrEmpty(message) ? "" : Environment.NewLine + message);
        }

        protected bool IsMatch(TToken tokenType)
        {
            return !IsEndOfInput() && tokenType.Equals(Peek().Type);
        }

        protected bool IsMatchOneOf(params TToken[] tokenTypes)
        {
            return !IsEndOfInput() && tokenTypes.Select(item => item.Equals(Peek().Type)).FirstOrDefault();
        }

        protected bool IsMatch(string value, bool ignoreCase = false)
        {
            if (!IsEndOfInput())
            {
                return ignoreCase ? string.Equals(value, Peek().Value, StringComparison.OrdinalIgnoreCase) : value == Peek().Value;
            }
            return false;
        }

        protected bool IsMatchOneOf(bool ignoreCase = false, params string[] values)
        {
            if (ignoreCase)
            {
                return values.Select(item => string.Equals(item, Peek().Value, StringComparison.OrdinalIgnoreCase)).Any();
            }
            return !IsEndOfInput() && values.Select(item => item == Peek().Value).Any();
        }
    }
}