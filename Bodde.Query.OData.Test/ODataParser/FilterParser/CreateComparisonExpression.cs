using Sut = Bodde.Query.OData.Internals.ODataParser.FilterParser;

namespace ODataParser.FilterParser;

public class CreateComparisonExpression
{
    [Fact]
    public void NotBinary_Throw()
    {
        var input = "2 gt Id lt 5";
        var expectedMessage = "Binary comparison expressions only are supported.";
        
        var actual = Assert.Throws<FormatException>(() => Sut.CreateComparisonExpression(input));

        Assert.Equal(expectedMessage, actual.Message);
    }
}
