namespace ParserToolkit;

public class Error
{
    public int Line { get; }
    public int Column { get; }
    public string Message { get; }
    public string Expected { get; }
    public string Found { get; }

    public Error(int line, int column, string message, string expected, string found)
    {
        Line = line;
        Column = column;
        Message = message;
        Expected = expected;
        Found = found;
    }

    public override string ToString()
    {
        var baseError = $"Error at {Line}:{Column} - {Message}";
        if (!string.IsNullOrWhiteSpace(Expected) && !string.IsNullOrWhiteSpace(Found))
        {
            baseError += $" (Expected: {Expected}, Found: {Found})";
        }
        return baseError;
    }
}