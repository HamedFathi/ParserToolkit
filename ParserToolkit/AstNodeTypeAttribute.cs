namespace ParserToolkit;

[AttributeUsage(AttributeTargets.Class)]
public class AstNodeTypeAttribute : Attribute
{
    public string Name { get; }

    public AstNodeTypeAttribute(string name)
    {
        Name = name;
    }
}