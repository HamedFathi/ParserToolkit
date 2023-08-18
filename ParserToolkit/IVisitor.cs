// ReSharper disable UnusedTypeParameter

namespace ParserToolkit;

public interface IVisitor<TToken, TResult>
    where TResult : AstNode<TToken>
    where TToken : Enum
{

}