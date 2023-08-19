// ReSharper disable UnusedMember.Global

namespace ParserToolkit;

public class Position
{
    public Position(string value, int position, int line, int column)
    {
        Line = line;
        Column = column;
        Value = value;
        End = position;
    }
    public int Column { get; }
    public int End { get; }
    public bool IsMultiLine => Value.Contains('\n');
    public int Length => Value.Length;
    public int Line { get; }
    public int Start => End - Length;
    public string Value { get; }
}