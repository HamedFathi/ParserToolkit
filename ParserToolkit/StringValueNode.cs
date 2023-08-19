// ReSharper disable UnusedMember.Global
namespace ParserToolkit;

public class StringValueNode<TToken> : AstNode<TToken> where TToken : Enum
{
    public TToken Token { get; set; }
    public string Value { get; set; }

    public StringValueNode(TToken token, string value)
    {
        Value = value;
        Token = token;
    }
}