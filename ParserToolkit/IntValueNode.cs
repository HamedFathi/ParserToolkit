// ReSharper disable UnusedMember.Global
namespace ParserToolkit;

public class IntValueNode<TToken> : AstNode<TToken> where TToken : Enum
{
    public TToken Token { get; set; }
    public int Value { get; set; }

    public IntValueNode(TToken token, int value)
    {
        Token = token;
        Value = value;
    }
}