using System.Linq.Expressions;
using Bodde.Query.OData.Internals;
using Bodde.Query.OData.Test.Models;
using static Bodde.Query.OData.Internals.ExpressionBuilder;

namespace ExpressionBuilder.PredicateBuilder;

public class CreateLogicalExpression
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void FirstOrSecondExpressionsNull_InvalidOperationException(bool firstNull, bool secondNull)
    {
#pragma warning disable CS8604 // Possible null reference argument.
        ComparisonExpression? first = firstNull ? null : new ComparisonExpression("Id", ComparisonOperator.GreaterThan, 1);
        ComparisonExpression? second = secondNull ? null : new ComparisonExpression("FirstName", ComparisonOperator.Equals, "John");
        var input = new LogicalExpression(LogicalOperator.And, first, second);
        var expectedMessage = "At least two expressions must be provided for a logical expression.";

        var sut = new PredicateBuilder<Employee>(Expression.Parameter(typeof(Employee)));
        var actual = Assert.Throws<InvalidOperationException>(() => sut.CreateLogicalExpression(input));

        Assert.Equal(expectedMessage, actual.Message);
#pragma warning restore CS8604 // Possible null reference argument.
    }

    [Fact]
    public void UnknownLogicalOperator_InvalidOperationException()
    {
        var first = new ComparisonExpression("Id", ComparisonOperator.GreaterThan, 1);
        var second = new ComparisonExpression("FirstName", ComparisonOperator.EndsWith, "John");
        var input = new LogicalExpression((LogicalOperator)999, first, second);
        var expectedMessage = "Logical operator 999 is not supported.";

        var sut = new PredicateBuilder<Employee>(Expression.Parameter(typeof(Employee)));
        var actual = Assert.Throws<InvalidOperationException>(() => sut.CreateLogicalExpression(input));

        Assert.Equal(expectedMessage, actual.Message);
    }
}
