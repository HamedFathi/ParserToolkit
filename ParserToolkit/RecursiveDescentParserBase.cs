using System.Text.RegularExpressions;
// ReSharper disable UnusedMember.Global

namespace ParserToolkit;

public static class Extensions
{
    public static bool IsRegexMatch(this string input, string regex)
    {
        return Regex.Match(input, regex, RegexOptions.Compiled).Success;
    }

    public static bool IsRegexMatch(this string input, Regex regex)
    {
        return regex.Match(input).Success;
    }
}
public class Error
{
    public Error(int line, int column, string message, string expected, string found)
    {
        Line = line;
        Column = column;
        Message = message;
        Expected = expected;
        Found = found;
    }

    public int Column { get; }
    public string Expected { get; }
    public string Found { get; }
    public int Line { get; }
    public string Message { get; }
    public override string ToString()
    {
        return $"Error at {Line}:{Column} - {Message}, (Expected: {Expected}, Found: {Found})";
    }
}

public abstract class LexerBase<TToken> where TToken : Enum
{
    private const char EndOfFile = '\0';
    private readonly List<Error>? _errors = new();
    private readonly string _input;
    private readonly List<Token<TToken>> _tokens = new();
    protected LexerBase(string input)
    {
        _input = input;
    }

    // = 0
    protected int Column { get; set; }

    protected int Line { get; set; } = 1;

    // = 0
    protected int Position { get; set; }
    protected char Value { get; set; }
    public LexerResult<TToken> Tokenize()
    {
        while (!IsEndOfFile())
        {
            ReadToken();
        }

        return _errors != null && _errors.Any()
            ? new LexerResult<TToken> { Input = _input, Tokens = null, Errors = _errors }
            : new LexerResult<TToken> { Input = _input, Tokens = _tokens, Errors = null };
    }

    protected void AddError(Token<TToken> currentToken, string expected, string message = "")
    {
        if (currentToken == null)
            throw new ArgumentNullException($"'{nameof(currentToken)}' argument is null.");


        if (string.IsNullOrWhiteSpace(expected))
            throw new ArgumentNullException($"'{nameof(expected)}' argument is null or empty.");

        _errors?.Add(new Error(currentToken.Position.Line, currentToken.Position.Column, message, expected, currentToken.Value));
    }

    protected void AddToken(Token<TToken> token)
    {
        _tokens.Add(token);
    }

    protected char Expect(char c, bool ignoreCase = false)
    {
        var value = Peek();

        if (ignoreCase)
        {
            if (char.ToLowerInvariant(value) != char.ToLowerInvariant(c))
                throw new Exception($"Expected '{c}' but got '{value}' at position {Position}.");
        }
        else
        {
            if (value != c)
                throw new Exception($"Expected '{c}' but got '{value}' at position {Position}.");
        }
        return Read();

    }

    // EOF
    protected bool IsEndOfFile()
    {
        return Position >= _input.Length || Value == EndOfFile;
    }

    protected bool IsEndOfFile(char ch)
    {
        return ch == EndOfFile;
    }

    protected virtual bool IsIdentifierCandidate(char ch, Regex? regex = null)
    {
        var re = regex ?? new Regex("^[_a-zA-Z0-9]+$", RegexOptions.Compiled);
        return ch.ToString().IsRegexMatch(re);
    }

    protected bool IsMatch(char ch)
    {
        if (!IsEndOfFile())
        {
            return ch == Peek();
        }
        return false;
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

    protected char Peek()
    {
        if (Position < _input.Length)
        {
            return _input[Position];
        }
        else
        {
            return EndOfFile;
        }
    }

    protected char[] Peek(int lookahead)
    {
        if (lookahead < 1)
        {
            throw new Exception($"'{nameof(lookahead)}' should be greater than 0.");
        }
        var list = new List<char>();
        for (var i = 0; i < lookahead; i++)
        {
            var pos = Position + i;
            if (pos < _input.Length)
            {
                list.Add(_input[pos]);
            }
            else
            {
                list.Add(EndOfFile);
                break;
            }
        }
        return list.ToArray();
    }

    protected string PeekAsString()
    {
        return Peek().ToString();
    }

    protected string PeekAsString(int lookahead)
    {
        return new string(Peek(lookahead));
    }

    // Next
    // Advance
    // Consume
    protected char Read()
    {
        var value = Peek();
        Position++;
        Value = value;
        switch (value)
        {
            case EndOfFile:
                return EndOfFile;
            case '\n':
                Line++;
                Column = 0;
                break;
            default:
                Column++;
                break;
        }

        return value;
    }

    protected bool Read(char c, bool ignoreCase = false)
    {
        var value = Peek();

        if (ignoreCase)
        {
            if (char.ToLowerInvariant(value) != char.ToLowerInvariant(c))
                return false;
        }
        else if (value != c)
            return false;

        Read();
        return true;

    }

    protected char[] Read(int lookahead)
    {
        if (lookahead < 1)
        {
            throw new Exception($"'{nameof(lookahead)}' should be greater than 0.");
        }
        var list = new List<char>();
        for (var i = 0; i < lookahead; i++)
        {
            var value = Peek();
            Position++;
            Value = value;
            list.Add(value);

            if (value == EndOfFile)
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

    protected string ReadAsString()
    {
        return Read().ToString();
    }

    protected string ReadAsString(int lookahead)
    {
        return new string(Read(lookahead));
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

    protected string ReadLine(bool movePointerToNextLine = false)
    {
        var result = ReadWhile(ch => ch != '\n');
        if (movePointerToNextLine) Skip();
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

    // The only way to add tokens.
    protected abstract void ReadToken();
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
    protected void Skip()
    {
        Read();
    }

    protected void SkipLine(bool movePointerToNextLine = false)
    {
        if (movePointerToNextLine)
            ReadLine(true);
        else
            ReadLine();
    }

    protected void SkipWhile(Func<char, bool> predicate)
    {
        ReadWhile(predicate);
    }
}

public sealed class LexerResult<TToken>
    where TToken : Enum
{
    public IEnumerable<Error>? Errors { get; internal set; } = new List<Error>();
    public string Input { get; internal set; } = "";
    public IList<Token<TToken>>? Tokens { get; internal set; } = new List<Token<TToken>>();
}

public class ParserResult<TResult> where TResult : class, new()
{
    public IEnumerable<Error>? Errors { get; internal set; }
    public TResult? Result { get; internal set; }
}

public class Position
{
    private readonly string _value;
    public Position(string value, int position, int line, int column)
    {
        Line = line;
        Column = column;
        _value = value;
        End = position;
    }
    public int Column { get; }
    public int End { get; }
    public bool IsMultiLine => _value.Contains('\n');
    public int Length => _value.Length;
    public int Line { get; }
    public int Start => End - Length;
    public string Value => _value;
}

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
public abstract class RecursiveDescentParserBase<TToken, TResult>
    where TToken : Enum
    where TResult : class, new()
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

    public ParserResult<TResult> Parse()
    {
        var result = Process();

        return _errors.Any()
            ? new ParserResult<TResult>
            {
                Result = result,
                Errors = _errors
            }
            : new ParserResult<TResult>
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

    protected abstract TResult Process();
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