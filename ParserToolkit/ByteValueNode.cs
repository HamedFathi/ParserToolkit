// ReSharper disable UnusedMember.Global

namespace ParserToolkit;

public class ByteValueNode<TToken> : AstNode<TToken> where TToken : Enum
{
    public TToken Token { get; set; }
    public byte Value { get; set; }

    public ByteValueNode(TToken token, byte value)
    {
        Token = token;
        Value = value;
    }
}