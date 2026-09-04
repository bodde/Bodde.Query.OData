using System.Collections.Specialized;
using System.Linq.Expressions;
using Bodde.Query.OData.Test.Models;
using static Bodde.Query.OData.Internals.ExpressionBuilder;

namespace ExpressionBuilder.PredicateBuilder;

public class GetInValues
{
    [Fact]
    public void Value_Not_Collection_ThrowsInvalidOperationException()
    {
        var expectedMessage = "Value for 'In' operator must be a collection.";
        var propertyExpression = Expression.Property(Expression.Parameter(typeof(Employee)), "Id");
        var value = 10;
        var actual = Assert.Throws<InvalidOperationException>(() => PredicateBuilder<Employee>.GetInValues(propertyExpression, value));

        Assert.Equal(expectedMessage, actual.Message);
    }


    [Fact]
    public void Value_ElementType_Equals_PropertyType_ReturnsValue()
    {
        var propertyExpression = Expression.Property(Expression.Parameter(typeof(Employee)), "Id");
        var value = new long[] { 10, 11 };
        var actual = PredicateBuilder<Employee>.GetInValues(propertyExpression, value);

        Assert.Equal(value, actual);
    }
}
