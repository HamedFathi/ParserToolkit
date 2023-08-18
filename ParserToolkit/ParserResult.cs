// ReSharper disable UnusedMember.Global

namespace ParserToolkit;

public class ParserResult<TToken> where TToken : Enum
{
    public ParserResult(AstNode<TToken> result, IEnumerable<Error>? errors)
    {
        Result = result;
        Errors = errors;
    }

    public AstNode<TToken> Result { get; internal set; }
    public IEnumerable<Error>? Errors { get; internal set; }
    public bool HasErrors => Errors != null && Errors.Any();

}