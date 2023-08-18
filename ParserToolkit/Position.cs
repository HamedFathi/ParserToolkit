// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace

namespace ParserKit.Examples.SourceCode;

public class Position
{
    public Position(string value, int position, int line, int column)
    {
        Line = line;
        Column = column;
        Value = value;
        End = position;
    }
    public string Value { get; }
    public int Line { get; }
    public int Column { get; }
    public int Length => Value.Length;
    public int End { get; }
    public int Start => End - Length;
    public bool IsMultiLine => Value.Contains('\n');
}