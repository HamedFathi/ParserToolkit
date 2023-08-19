// ReSharper disable UnusedMember.Global
using System.Text.RegularExpressions;

namespace ParserToolkit;

public abstract class LexerBase<TToken> where TToken : Enum
{
    private const char EndOfFile = '\0';
    private readonly List<Error>? _errors = new();
    private readonly string _input;
    private readonly List<Token<TToken>> _tokens = new();
    protected LexerBase(string input)
    {
        _input = input;
        Value = null;
    }

    // = 0
    protected int Column { get; set; }

    protected int Line { get; set; } = 1;

    // = 0
    protected int Position { get; set; }
    protected char? Value { get; set; }
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
        return Position < _input.Length ? _input[Position] : EndOfFile;
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