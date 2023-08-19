namespace ParserToolkit.Test;

public class LexerAndParserTests
{
    [Fact]
    public void Lexer_ShouldTokenizeSimpleExpression_Successfully()
    {
        // Arrange
        var lexer = new ArithmeticLexer("2+3"); // Replace ArithmeticLexer with your actual Lexer implementation

        // Act
        var result = lexer.Tokenize();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Errors);
        Assert.NotEmpty(result.Tokens);
    }

    //[Fact]
    //public void Lexer_ShouldReturnError_OnInvalidInput()
    //{
    //    // Arrange
    //    var lexer = new ArithmeticLexer("2++3");

    //    // Act
    //    var result = lexer.Tokenize();

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.NotNull(result.Errors);
    //    Assert.NotEmpty(result.Errors);
    //}

    [Fact]
    public void Parser_ShouldParseSimpleExpression_Successfully()
    {
        // Arrange
        var lexer = new ArithmeticLexer("2+3");
        var lexerResult = lexer.Tokenize();
        var parser = new ArithmeticParser(lexerResult); // Replace ArithmeticParser with your actual Parser implementation

        // Act
        var parseResult = parser.Parse();

        // Assert
        Assert.NotNull(parseResult);
        Assert.Null(parseResult.Errors);
        Assert.NotNull(parseResult.Result);
    }
}