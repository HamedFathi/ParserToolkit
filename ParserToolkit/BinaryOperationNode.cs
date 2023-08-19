namespace ParserToolkit;

public class BinaryOperationNode<T> : AstNode<T> where T : Enum
{
    public AstNode<T> Left { get; set; }
    public T Operator { get; set; }
    public AstNode<T> Right { get; set; }

    public BinaryOperationNode(AstNode<T> left, T op, AstNode<T> right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }
}