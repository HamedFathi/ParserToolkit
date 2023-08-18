namespace ParserToolkit;

public abstract class AstNode<TToken> where TToken : Enum
{
    public Token<TToken> Token { get; protected set; }

    protected AstNode(Token<TToken> token)
    {
        Token = token;
    }

    public abstract TResult Accept<TResult>(IVisitor<TToken, TResult> visitor) where TResult : AstNode<TToken>;
}