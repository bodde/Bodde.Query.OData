using Sut = Bodde.Query.OData.Internals.ODataParser.FilterParser;

namespace ODataParser.FilterParser;
public class GetNotInnerExpressionKey
{
    [Theory]
    [InlineData("not |0|", "|0|")] 
    [InlineData("not (|1|)", "|1|")]
    [InlineData("not(|2|)", "|2|")] 
    public void InnerExpression_Found(string input, string expected)
    {
        var groups = Sut.NotExpressionsRegex.Match(input).Groups;

        var actual = Sut.GetNotInnerExpressionKey(groups);

        Assert.Equal(expected, actual);
    }

    [Theory] 
    [InlineData("not x eq 3")]   
    [InlineData("not (x eq 3)")]   
    [InlineData("not(x eq 3)")]   
    public void NotInnerExpression_Not_Found_Throw(string input)
    {
        var groups = Sut.NotExpressionsRegex.Match(input).Groups;
        var expectedMessage = "No valid group found for not expression.";
        
        var actual = Assert.Throws<FormatException>(() => Sut.GetNotInnerExpressionKey(groups));

        Assert.Equal(expectedMessage, actual.Message);
    }
}
