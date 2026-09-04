using Sut = Bodde.Query.OData.Internals.ODataParser.FilterParser;

namespace ODataParser.FilterParser;

public class ParseValue
{
    [Theory]
    [InlineData("null", null, typeof(object))]
    [InlineData("true", true, typeof(bool))]
    [InlineData("false", false, typeof(bool))]
    [InlineData("True", true, typeof(bool))]
    [InlineData("False", false, typeof(bool))]
    [InlineData("123", 123, typeof(int))]
    [InlineData("123.45", 123.45, typeof(double))]
    [InlineData("'Hello, World!'", "Hello, World!", typeof(string))]
    [InlineData("2020/01/01T00:00:00Z", "2020/01/01T00:00:00Z", typeof(DateTime))]
    public void ValidInputs(string input, object? expectedValue, Type expectedType)
    {
        if(expectedType == typeof(DateTime) && expectedValue is string dateTimeString)
        {
            expectedValue = DateTime.Parse(dateTimeString).ToUniversalTime();
        }

        var (result, type) = Sut.ParseValue(input);
        Assert.Equal(expectedValue, result);
        Assert.Equal(expectedType, type);
    }

    [Fact]
    public void UnknownInputType_ThrowsNotImplementedException()
    {
        var input = "unknownTypeValue";

        Assert.Throws<NotImplementedException>(() => Sut.ParseValue(input));
    }
}
