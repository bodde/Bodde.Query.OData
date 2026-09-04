using System.Linq.Expressions;
using Bodde.Query.OData.Internals;
using Bodde.Query.OData.Test.Models;
using static Bodde.Query.OData.Internals.ExpressionBuilder;

namespace ExpressionBuilder.PredicateBuilder;

public class CreateOperatorExpression
{
    [Fact]
    public void UnknownOperator_ThrowsNotSupportedException()
    {
        var constant = Expression.Constant(10, typeof(int));
        var propertyExpression = Expression.Property(Expression.Parameter(typeof(Employee)), "Id");
        var comparisonExpression = new ComparisonExpression("Id", (ComparisonOperator)999, 10);
        var expectedMessage = "Operator 999 is not supported.";

        var sut = new PredicateBuilder<Employee>(Expression.Parameter(typeof(Employee)));
        var actual = Assert.Throws<NotSupportedException>(() => sut.CreateOperatorExpression(comparisonExpression, propertyExpression, constant));

        Assert.Equal(expectedMessage, actual.Message);
    }
}
