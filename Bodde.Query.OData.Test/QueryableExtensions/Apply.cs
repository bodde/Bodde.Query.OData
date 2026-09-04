using Bodde.Common.Extensions;
using Bodde.Query.OData;
using Bodde.Query.OData.Test;
using Bodde.Query.OData.Test.Models;

namespace QueryableExtensions;

public class Apply
{    
    private readonly IQueryable<Employee> data;

    public Apply()
    {
        data = EmployeeSetBuilder.Build().AsQueryable();
    }
    
    [Theory]
    [InlineData(null, null, null, null, "1,2,3,4,5,6,7,8,9,10")]
    [InlineData("Id eq 5", null, null, null, "5")]    
    [InlineData("IsActive eq true", null, null, null, "1,2,3,4,5,6,7,9")]
    [InlineData("FirstName eq 'Emma'", null, null, null, "1")]
    [InlineData("FirstName eq 'James'", null, null, null, "6,8")]
    [InlineData("FirstName ne 'Emma'", null, null, null, "2,3,4,5,6,7,8,9,10")]
    [InlineData("FirstName startswith 'J'", null, null, null, "6,8")]
    [InlineData("LastName endswith 'on'", null, null, null, "1,6,9")]
    [InlineData("Email contains 'on@example'", null, null, null, "1,6,9")]
    [InlineData("HireDateTimeOffset gt '2020/01/01T00:00:00Z'", null, null, null, "3,5,9")]
    [InlineData("Id ge 5", null, null, null, "5,6,7,8,9,10")]
    [InlineData("Id lt 5", null, null, null, "1,2,3,4")]
    [InlineData("Id le 5", null, null, null, "1,2,3,4,5")]
    [InlineData("Department.Name eq 'Engineering'", null, null, null, "1,4,5,9")]
    [InlineData("Role eq 'Manager'", null, null, null, "2,7,10")]
    [InlineData("Role in ('Developer', 'Analyst')", null, null, null, "1,3,4,5,6,8,9")]
    [InlineData("Role eq 'Manager' and Department.Name eq 'Design'", null, null, null, "7,10")] 
    [InlineData("Role eq 'Manager' and Department.Name eq 'Design' and Id lt 10", null, null, null, "7")] 
    [InlineData("not(IsActive eq true)", null, null, null, "8,10")]
    [InlineData("not Role in ('Developer', 'Analyst')", null, null, null, "2,7,10")]
    [InlineData("not Email contains 'on@example'", null, null, null, "2,3,4,5,7,8,10")]
    [InlineData("not (Role in ('Developer', 'Analyst') and Department.Name eq 'Engineering')", null, null, null, "2,3,6,7,8,10")]
    [InlineData("Role eq 'Manager' or Department.Name eq 'Design'", null, null, null, "2,3,7,10")] 
    [InlineData("(Role eq 'Manager' or Department.Name eq 'Design') and IsActive eq true", null, null, null, "2,3,7")] 
    [InlineData("not((Role eq 'Manager' or Department.Name eq 'Design') and IsActive eq true)", null, null, null, "1,4,5,6,8,9,10")] 
    [InlineData(null, "Id desc", null, null, "10,9,8,7,6,5,4,3,2,1")]
    [InlineData(null, "FirstName, Id", null, null, "5,10,1,9,6,8,2,4,3,7")]
    [InlineData(null, "Department.Name DESC, Id", null, null, "2,6,8,1,4,5,9,3,7,10")]
    [InlineData(null, "Department.Name, Id desc", null, null, "10,7,3,9,5,4,1,8,6,2")]
    [InlineData(null, null, 1, null, "2,3,4,5,6,7,8,9,10")]
    [InlineData(null, null, null, 3, "1,2,3")]
    [InlineData(null, null, 3, 5, "4,5,6,7,8")]
    [InlineData(null, null, 3, 100, "4,5,6,7,8,9,10")]
    [InlineData(null, null, 100, null, "")]
    [InlineData("Id ge 2", null, 3, 5, "5,6,7,8,9")]
    [InlineData("Id ge 2", "Id desc", 3, 5, "7,6,5,4,3")]
    public void Sunny(string? filter, string? orderby, int? skip, int? top, string expectedCsv)
    {
        var result = data.Apply(new(filter, orderby, skip, top)).ToArray()!;

        var actualCsv = result.ToCsv(_ => _.Id.ToString());

        Assert.Equal(expectedCsv, actualCsv);
    }


    [Theory]
    [InlineData("x", "No valid comparison statements found in filter string.")]
    [InlineData("Id xx 3", "OData operator 'xx' is not supported. Supported operators are: eq,ne,gt,ge,lt,le,contains,startswith,endswith,in.")]
    [InlineData("Id eq 1 and", "Unable to create top-level expression from filter string.")]
    [InlineData("Id eq 1 and ()", "At least two expressions are required to create a logical expression.")]
    [InlineData("Id gt 1 and Id lt 5 or Id eq 10", "Only one logical operator per logical expression is supported.")]
    [InlineData("not()", "No valid comparison statements found in filter string.")]
    [InlineData("not(x)", "No valid comparison statements found in filter string.")]
    [InlineData("Id eq 1 and not()", "At least two expressions are required to create a logical expression.")]
    [InlineData("Id in ()", "No valid comparison statements found in filter string.")]
    [InlineData("Id in 1,2", "Invalid syntax for 'in' operator.")]
    [InlineData("Id in (1,'2')", "All values for 'in' operator must be of the same type.")]
    [InlineData("Id eq 1 gt 2", "Unable to create top-level expression from filter string.")]
    public void InvalidFilter_FormatException(string filter, string expectedMessage)
    {
        var actual = Assert.Throws<FormatException>(() => data.Apply(new(filter)));

        Assert.Equal(expectedMessage, actual.Message);
    }
}
