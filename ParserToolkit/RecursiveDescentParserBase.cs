// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedTypeParameter

namespace ParserToolkit;

public abstract class RecursiveDescentParserBase<TToken>
    where TToken : Enum
{
    private readonly IList<Error> _errors = new List<Error>();
    private readonly IList<Token<TToken>> _tokens;
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

    public ParserResult<TToken> Parse()
    {
        var result = Process();

        return _errors.Any()
            ? new ParserResult<TToken>
            {
                Result = result,
                Errors = _errors
            }
            : new ParserResult<TToken>
            {
                Result = result,
                Errors = null
            };
    }

    protected void AddError(Token<TToken> currentToken, string expected, string message = "")
    {
        if (currentToken == null)
            throw new ArgumentNullException($"'{nameof(currentToken)}' argument is null.");


        if (string.IsNullOrWhiteSpace(expected))
            throw new ArgumentNullException($"'{nameof(expected)}' argument is null or empty.");

        _errors.Add(new Error(currentToken.Position.Line, currentToken.Position.Column, message, expected, currentToken.Value));
    }

    protected Token<TToken>? Expect(Token<TToken> token, bool ignoreCase = false)
    {
        var current = Peek();
        if (ignoreCase)
        {
            if (!string.Equals(token.Value, current?.Value, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Expected '{token.Value}' but got '{current?.Value}' at position {_position}.");
            }
        }
        else if (!string.Equals(token.Value, current?.Value))
        {
            throw new Exception($"Expected '{token.Value}' but got '{current?.Value}' at position {_position}.");
        }

        return Read();
    }

    protected bool IsEndOfInput()
    {
        return _tokens.Count - 1 < _position;
    }

    protected bool IsMatch(TToken tokenType)
    {
        var peek = Peek();
        if (peek == null) return false;
        return !IsEndOfInput() && tokenType.Equals(peek.Type);
    }

    protected bool IsMatch(params TToken[] tokenTypes)
    {
        var peek = Peek();
        if (peek == null) return false;
        return !IsEndOfInput() && tokenTypes.Select(item => item.Equals(peek.Type)).FirstOrDefault();
    }

    protected bool IsMatch(string value, bool ignoreCase = false)
    {
        if (!IsEndOfInput())
        {
            return ignoreCase ? string.Equals(value, Peek()?.Value, StringComparison.OrdinalIgnoreCase) : value == Peek()?.Value;
        }
        return false;
    }

    protected bool IsMatch(bool ignoreCase = false, params string[] values)
    {
        if (IsEndOfInput()) return false;
        return ignoreCase
            ? values.Select(item => string.Equals(item, Peek()?.Value, StringComparison.OrdinalIgnoreCase)).Any()
            : values.Select(item => item == Peek()?.Value).Any();
    }

    protected Token<TToken>? Peek()
    {
        return !IsEndOfInput() ? _tokens[_position] : null;
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
        for (var i = 0; i < lookahead; i++)
        {
            result.Add(_tokens[i + _position]);
        }
        return result.ToArray();
    }

    protected abstract AstNode<TToken> Process();
    protected Token<TToken>? Read()
    {
        if (IsEndOfInput()) return null;
        var result = Peek();
        _position++;
        return result;
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
        for (var i = 0; i < lookahead; i++)
        {
            var read = Read();
            if (read != null)
                result.Add(read);
        }
        return result.ToArray();
    }
    protected bool Read(Token<TToken> token, bool ignoreCase = false)
    {
        var current = Peek();
        if (ignoreCase)
        {
            if (!string.Equals(token.Value, current?.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        else if (!string.Equals(token.Value, current?.Value))
        {
            return false;
        }

        Read();
        return true;
    }
    protected T[] ReadWhile<T>(Func<T, bool> predicate) where T : Token<TToken>
    {
        var peek = Peek();
        if (peek == null) return Array.Empty<T>();

        var result = new List<T>();
        while (!IsEndOfInput() && predicate((T)peek))
        {
            var read = Read();
            if (read != null)
                result.Add((T)read);
        }
        return result.ToArray();
    }

    // More functionality
    protected void Skip()
    {
        Read();
    }
    protected void SkipWhile<T>(Func<T, bool> predicate) where T : Token<TToken>
    {
        ReadWhile(predicate);
    }

    protected void Traverse(Action<Token<TToken>> action)
    {
        foreach (var token in _tokens)
        {
            action(token);
        }
    }
}