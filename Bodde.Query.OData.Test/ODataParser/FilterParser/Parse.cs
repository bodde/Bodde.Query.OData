using Sut = Bodde.Query.OData.Internals.ODataParser.FilterParser;

namespace ODataParser.FilterParser;

public class Parse
{
    [Fact]
    public void Null_Argument_Throw()
    {
#pragma warning disable CS8604 // Possible null reference argument.
        string? input = null;
        var expectedMessage = "Value cannot be null. (Parameter 'filter')";
        var sut = new Sut();
        var actual = Assert.Throws<ArgumentNullException>(() => sut.Parse(input));

        Assert.Equal(expectedMessage, actual.Message);
#pragma warning restore CS8604 // Possible null reference argument.
    }
}
