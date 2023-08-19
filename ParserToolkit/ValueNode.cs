namespace ParserToolkit;

public class ValueNode<TToken, TValue> : AstNode<TToken> where TToken : Enum
{
    public TToken Token { get; set; }
    public TValue Value { get; set; }

    public ValueNode(TToken token, TValue value)
    {
        Value = value;
        Token = token;
    }
}