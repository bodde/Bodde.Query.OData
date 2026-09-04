using Bodde.Query.OData.Test.Models;
using static Bodde.Query.OData.Internals.ExpressionBuilder;

namespace ExpressionBuilder.PredicateBuilder;

public class ConvertConstantValueToPropertyType
{
    [Fact]
    public void PropertyType_DateTimeOffset_Value_DateTime_Returns_DateTimeOffset()
    {
        var propertyType = typeof(DateTimeOffset);
        var value = DateTime.Now;
        var actual = PredicateBuilder<Employee>.ConvertConstantValueToPropertyType(value, propertyType);

        Assert.IsType<DateTimeOffset>(actual);
    }

    [Fact]
    public void PropertyType_DateTime_Value_DateTimeOffset_Returns_DateTime()
    {
        var propertyType = typeof(DateTime);
        var value = DateTimeOffset.Now;
        var actual = PredicateBuilder<Employee>.ConvertConstantValueToPropertyType(value, propertyType);

        Assert.IsType<DateTime>(actual);
    }

    [Fact]
    public void PropertyType_Bool_Value_String_Returns_Bool()
    {
        var propertyType = typeof(bool);
        var value = "true";
        var actual = PredicateBuilder<Employee>.ConvertConstantValueToPropertyType(value, propertyType);

        Assert.IsType<bool>(actual);
        Assert.True((bool)actual);
    }

    [Fact]
    public void PropertyType_Bool_Value_Bool_Returns_Bool()
    {
        var propertyType = typeof(bool);
        var value = true;
        var actual = PredicateBuilder<Employee>.ConvertConstantValueToPropertyType(value, propertyType);

        Assert.IsType<bool>(actual);
        Assert.True((bool)actual);
    }
}
