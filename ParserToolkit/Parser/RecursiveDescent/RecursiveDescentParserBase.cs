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
        private readonly IList<Token<TToken>> _tokens = new List<Token<TToken>>();
        private readonly IList<string> _errors = new List<string>();
        private int _position;

        protected RecursiveDescentParserBase(LexerResult<TToken> lexerResult)
        {
            if (lexerResult.Tokens == null || lexerResult.Tokens.Count == 0)
            {
                throw new Exception("The lexer result has no token.");
            }

            // Iterates through our tokens (collection).
            _tokens = lexerResult.Tokens;
            _position = 0;
        }

        protected abstract TResult Process();

        public ParserResult<TResult> Parse()
        {
            var result = Process();

            return new ParserResult<TResult>
            {
                Errors = _errors.Count > 0 ? _errors : null,
                Result = result
            };

        }

        protected void AddError(Token<TToken> currentToken, string expected, string message = "")
        {
            if (currentToken == null)
                throw new ArgumentNullException($"'{nameof(currentToken)}' argument is null.");


            if (string.IsNullOrWhiteSpace(expected))
                throw new ArgumentNullException($"'{nameof(expected)}' argument is null or empty.");

            var error = Error(currentToken, expected, message);

            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentNullException("The 'Error()' function has returned null or empty.");

            _errors.Add(error);
        }
        protected Token<TToken> Read()
        {
            if (!IsEndOfInput())
            {
                var result = Peek();
                _position++;
                Visit(result);
                return result;
            }
            return null;
        }

        protected Token<TToken> Peek()
        {
            if (!IsEndOfInput())
                return _tokens[_position];
            else
                return null;
        }

        protected Token<TToken>[] Peek(int lookahead)
        {
            if (lookahead < 1)
            {
                throw new Exception($"'{nameof(lookahead)}' should be greater than 0.");
            }

            if (_tokens.Count - 1 < _position + lookahead)
            {
                throw new Exception($"'{nameof(lookahead)}' is bigger than the remaining tokens.");
            }

            var result = new List<Token<TToken>>();
            for (int i = 0; i < lookahead; i++)
            {
                result.Add(_tokens[i + _position]);
            }
            return result.ToArray();
        }
        protected Token<TToken>[] Read(int lookahead)
        {
            if (lookahead < 1)
            {
                throw new Exception($"'{nameof(lookahead)}' should be greater than 0.");
            }

            if (_tokens.Count - 1 < _position + lookahead)
            {
                throw new Exception($"'{nameof(lookahead)}' is bigger than the remaining tokens.");
            }

            var result = new List<Token<TToken>>();
            for (int i = 0; i < lookahead; i++)
            {
                result.Add(Read());
            }
            return result.ToArray();
        }

        protected bool IsEndOfInput()
        {
            return _tokens.Count - 1 < _position;
        }

        // More functionality
        protected void Skip()
        {
            Read();
        }

        protected T[] ReadWhile<T>(Func<T, bool> predicate) where T : Token<TToken>
        {
            var result = new List<T>();
            while (!IsEndOfInput() && predicate((T)Peek()))
            {
                result.Add((T)Read());
            }
            return result.ToArray();
        }

        protected void SkipWhile<T>(Func<T, bool> predicate) where T : Token<TToken>
        {
            ReadWhile(predicate);
        }

        protected virtual string Error(Token<TToken> currentToken, string expected, string message = "")
        {
            return
                    $"Expecting '{expected}' but got '{currentToken.Value}' ({currentToken.Position.Line}:{currentToken.Position.Column})"
                    + (string.IsNullOrWhiteSpace(message) ? "" : Environment.NewLine + message);
        }

        protected virtual void Visit(Token<TToken> currentToken)
        {

        }

        protected void Traverse(Action<Token<TToken>> action)
        {
            foreach (var token in _tokens)
            {
                action(token);
            }
        }

        protected bool IsMatch(TToken tokenType)
        {
            return !IsEndOfInput() && tokenType.Equals(Peek().Type);
        }

        protected bool IsMatch(params TToken[] tokenTypes)
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

        protected bool IsMatch(bool ignoreCase = false, params string[] values)
        {
            if (IsEndOfInput()) return false;
            if (ignoreCase)
            {
                return values.Select(item => string.Equals(item, Peek().Value, StringComparison.OrdinalIgnoreCase)).Any();
            }
            return values.Select(item => item == Peek().Value).Any();
        }
    }
}