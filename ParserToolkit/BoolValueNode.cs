// ReSharper disable UnusedMember.Global
namespace ParserToolkit;

public class BoolValueNode<TToken> : AstNode<TToken> where TToken : Enum
{
    public TToken Token { get; set; }
    public bool Value { get; set; }

    public BoolValueNode(TToken token, bool value)
    {
        Token = token;
        Value = value;
    }
}