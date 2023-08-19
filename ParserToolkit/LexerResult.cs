namespace ParserToolkit;

public sealed class LexerResult<TToken>
    where TToken : Enum
{
    public IEnumerable<Error>? Errors { get; internal set; } = new List<Error>();
    public string Input { get; internal set; } = "";
    public IList<Token<TToken>>? Tokens { get; internal set; } = new List<Token<TToken>>();
}