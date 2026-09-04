using Bodde.Query.OData.Test.Models;
using static Bodde.Query.OData.Internals.ExpressionBuilder;

namespace ExpressionBuilder.PredicateBuilder;

public class ConvertInValues
{
    [Fact]
    public void Value_NotEnumerable_ThrowsArgumentException()
    {
        var propertyType = typeof(int);
        var value = DateTime.Now;

        var actual = Assert.Throws<ArgumentException>(() => PredicateBuilder<Employee>.ConvertInValues(value, propertyType));
        
        Assert.Equal("Value for 'In' operator must be a IEnumerable collection.", actual.Message);
    }
}
