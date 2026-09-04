using Sut = Bodde.Query.OData.Internals.ODataParser.FilterParser;

namespace ODataParser.FilterParser;

public class ConvertLogicalOperatorStringToEnum
{
    [Fact]    
    public void NotSupported_Operator_Throw()
    {
        var input = "xx";
        var expectedMessage = "Logical operator 'xx' is not supported.";
        
        var actual = Assert.Throws<FormatException>(() => Sut.ConvertLogicalOperatorStringToEnum(input));

        Assert.Equal(expectedMessage, actual.Message);
    }
}
