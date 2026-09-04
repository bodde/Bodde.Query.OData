using Bodde.Query.OData.Internals;
using Bodde.Query.OData.Test.Models;
using Moq;
using Sut = Bodde.Query.OData.Internals.ODataParser;

namespace ODataParser;

public class ParseOrderBy
{    
    private readonly Mock<IExpressionBuilder> expressionBuilder; 
    private readonly Sut sut;

    public ParseOrderBy()
    {
        expressionBuilder = new();
        sut = new(expressionBuilder.Object);
    }


    [Fact]    
    public void ArgumentNullException()
    {
#pragma warning disable CS8604 // Possible null reference argument.
        string? nullArgument = null;
        var actual = Assert.Throws<ArgumentNullException>(() => sut.ParseOrderBy<Employee>(nullArgument));
#pragma warning restore CS8604 // Possible null reference argument.
    }
}
