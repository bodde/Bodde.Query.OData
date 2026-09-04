using System.Linq.Expressions;
using Bodde.Query.OData.Internals;
using Bodde.Query.OData.Test.Models;
using static Bodde.Query.OData.Internals.ExpressionBuilder;

namespace ExpressionBuilder.PredicateBuilder;

public class BuildExpression
{
    [Fact]
    public void ArgumentNullException()
    {
#pragma warning disable CS8604 // Possible null reference argument.
        FilterExpression? input = null;
        var expectedMessage = "Value cannot be null. (Parameter 'filterExpression')";

        var sut = new PredicateBuilder<Employee>(Expression.Parameter(typeof(Employee)));
        var actual = Assert.Throws<ArgumentNullException>(() => sut.BuildExpression(input));

        Assert.Equal(expectedMessage, actual.Message);
#pragma warning restore CS8604 // Possible null reference argument.
    }

    [Fact]
    public void NotImplementedException()
    {
#pragma warning disable CS8604 // Possible null reference argument.
        var input = new NotImplementedExpression();
        var expectedMessage = "Filter expression type NotImplementedExpression is not implemented.";

        var sut = new PredicateBuilder<Employee>(Expression.Parameter(typeof(Employee)));
        var actual = Assert.Throws<NotImplementedException>(() => sut.BuildExpression(input));

        Assert.Equal(expectedMessage, actual.Message);
#pragma warning restore CS8604 // Possible null reference argument.
    }

    private record NotImplementedExpression: FilterExpression;
}
