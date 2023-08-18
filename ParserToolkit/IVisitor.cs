// ReSharper disable UnusedTypeParameter

namespace ParserToolkit;

public interface IVisitor<TToken, TResult>
    where TToken : Enum
    where TResult : AstNode<TToken>
{

}