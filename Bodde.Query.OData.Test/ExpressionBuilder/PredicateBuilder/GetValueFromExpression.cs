using System.Linq.Expressions;
using Bodde.Query.OData.Internals;
using Bodde.Query.OData.Test.Models;
using static Bodde.Query.OData.Internals.ExpressionBuilder;

namespace ExpressionBuilder.PredicateBuilder;

public class GetValueFromExpression
{
    [Fact]
    public void NullValue_ReturnsNull()
    {
        var propertyExpression = Expression.Property(Expression.Parameter(typeof(Employee)), "Id");
        var comparisonExpression = new ComparisonExpression("Id", ComparisonOperator.Equals, null);

        var actual = PredicateBuilder<Employee>.GetValueFromExpression(comparisonExpression, propertyExpression);

        Assert.Null(actual);
    }
}
