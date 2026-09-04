using System.Linq.Expressions;
using Bodde.Query.OData.Internals;
using Bodde.Query.OData.Test.Models;
using static Bodde.Query.OData.Internals.ExpressionBuilder;

namespace ExpressionBuilder.PredicateBuilder;

public class ValidateOperatorOrThrow
{
    [Theory]
    [InlineData(ComparisonOperator.StartsWith)]
    [InlineData(ComparisonOperator.EndsWith)]
    [InlineData(ComparisonOperator.Contains)]
    public void StringOperator_NonStringProperty_ThrowsInvalidOperationException(int op)
    {
        ComparisonOperator comparisonOperator = (ComparisonOperator)op;
        ComparisonExpression comparisonExpression = new("FirstName", comparisonOperator, "John");
        Expression propertyExpression = Expression.Property(Expression.Parameter(typeof(Employee)), "Role");

        var expectedMessage = $"Operator {comparisonOperator} can only be applied to string properties.";

        var actual = Assert.Throws<InvalidOperationException>(() => 
            PredicateBuilder<Employee>.ValidateOperatorOrThrow(comparisonExpression, propertyExpression)
            );

        Assert.Equal(expectedMessage, actual.Message);
    }
}
