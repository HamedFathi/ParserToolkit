// ReSharper disable UnusedMember.Global
namespace ParserToolkit.Test;
/*
    <expression> ::= <term> ("+" <term> | "-" <term>)*
    <term> ::= <factor> ("*" <factor> | "/" <factor>)*
    <factor> ::= <power> ("^" <power>)*
    <power> ::= <number> | "(" <expression> ")"
    <number> ::= [0-9]+

    Summary:

    1. Precedence (from highest to lowest):
       - `^`: Power
       - `*` and `/`: Multiplication and Division
       - `+` and `-`: Addition and Subtraction

    2. Associativity:
       - The power operation (`^`): Right-associative.
       - Multiplication (`*`), Division (`/`), Addition (`+`), and Subtraction (`-`): Left-associative.

    3. Parentheses:
       - Can be used to override default precedence and associativity rules.
       - An expression inside parentheses is evaluated first.

    4. Numbers:
       - Supported numbers are positive integers, consisting of one or more digits.
 */

public class ArithmeticParser : RecursiveDescentParserBase<ArithmeticToken>
{
    public ArithmeticParser(LexerResult<ArithmeticToken> lexerResult) : base(lexerResult) { }

    protected override AstNode<ArithmeticToken> Process()
    {
        var node = ParseExpression();
        return node;
    }

    private AstNode<ArithmeticToken> ParseExpression()
    {
        var node = ParseTerm();

        while (IsMatch(ArithmeticToken.Plus) || IsMatch(ArithmeticToken.Minus))
        {
            var token = Read();
            if (token == null)
            {
                throw new Exception($"Unexpected token: {Peek()?.Value}");
            }
            var right = ParseTerm();
            node = new BinaryOperationNode<ArithmeticToken>(node, token.Type, right);
        }

        return node;
    }

    private AstNode<ArithmeticToken> ParseTerm()
    {
        var node = ParseFactor();

        while (IsMatch(ArithmeticToken.Multiply) || IsMatch(ArithmeticToken.Divide))
        {
            var token = Read();
            if (token == null)
            {
                throw new Exception($"Unexpected token: {Peek()?.Value}");
            }
            var right = ParseFactor();
            node = new BinaryOperationNode<ArithmeticToken>(node, token.Type, right);
        }

        return node;
    }

    private AstNode<ArithmeticToken> ParseFactor()
    {
        var node = ParsePower();

        while (IsMatch(ArithmeticToken.Power))
        {
            var token = Read();
            if (token == null)
            {
                throw new Exception($"Unexpected token: {Peek()?.Value}");
            }
            var right = ParsePower();
            node = new BinaryOperationNode<ArithmeticToken>(node, token.Type, right);
        }

        return node;
    }

    private AstNode<ArithmeticToken> ParsePower()
    {
        if (IsMatch(ArithmeticToken.Number))
        {
            var token = Read();
            if (token == null)
            {
                throw new Exception($"Unexpected token: {Peek()?.Value}");
            }
            return new ValueNode<ArithmeticToken, double>(token.Type, Convert.ToDouble(token.Value));
        }

        if (IsMatch(ArithmeticToken.LeftParenthesis))
        {
            Read();
            var node = ParseExpression();
            Expect(new Token<ArithmeticToken>(ArithmeticToken.RightParenthesis, "", 0, 0, 0));
            return node;
        }
        throw new Exception($"Unexpected token: {Peek()?.Value}");
    }
}