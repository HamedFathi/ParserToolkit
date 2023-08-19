namespace ParserToolkit;

public class Error
{
    public Error(int line, int column, string message, string expected, string found)
    {
        Line = line;
        Column = column;
        Message = message;
        Expected = expected;
        Found = found;
    }

    public int Column { get; }
    public string Expected { get; }
    public string Found { get; }
    public int Line { get; }
    public string Message { get; }
    public override string ToString()
    {
        return $"Error at {Line}:{Column} - {Message}, (Expected: {Expected}, Found: {Found})";
    }
}