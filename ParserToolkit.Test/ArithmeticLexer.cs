namespace ParserToolkit.Test;

public class ArithmeticLexer : LexerBase<ArithmeticToken>
{
    public ArithmeticLexer(string input) : base(input) { }

    protected override void ReadToken()
    {
        while (char.IsWhiteSpace(Peek()))
            Read();

        if (IsEndOfFile())
        {
            AddToken(new Token<ArithmeticToken>(ArithmeticToken.Eof, "", Position, Line, Column));
            return;
        }

        var current = Peek();

        if (char.IsDigit(current))
        {
            var number = ReadWhile(char.IsDigit);
            AddToken(new Token<ArithmeticToken>(ArithmeticToken.Number, number, Position, Line, Column));
            return;
        }

        switch (current)
        {
            case '+': AddToken(new Token<ArithmeticToken>(ArithmeticToken.Plus, ReadAsString(), Position, Line, Column)); break;
            case '-': AddToken(new Token<ArithmeticToken>(ArithmeticToken.Minus, ReadAsString(), Position, Line, Column)); break;
            case '*': AddToken(new Token<ArithmeticToken>(ArithmeticToken.Multiply, ReadAsString(), Position, Line, Column)); break;
            case '/': AddToken(new Token<ArithmeticToken>(ArithmeticToken.Divide, ReadAsString(), Position, Line, Column)); break;
            case '^': AddToken(new Token<ArithmeticToken>(ArithmeticToken.Power, ReadAsString(), Position, Line, Column)); break;
            case '(': AddToken(new Token<ArithmeticToken>(ArithmeticToken.LeftParenthesis, ReadAsString(), Position, Line, Column)); break;
            case ')': AddToken(new Token<ArithmeticToken>(ArithmeticToken.RightParenthesis, ReadAsString(), Position, Line, Column)); break;
            default: throw new Exception($"Unexpected character: {current}");
        }
    }
}