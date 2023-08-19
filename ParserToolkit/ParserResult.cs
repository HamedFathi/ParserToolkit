namespace ParserToolkit;

public class ParserResult<TToken> where TToken : Enum
{
    public IEnumerable<Error>? Errors { get; internal set; }
    public AstNode<TToken>? Result { get; internal set; }
}