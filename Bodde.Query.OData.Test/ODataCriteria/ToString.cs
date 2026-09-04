
using Sut = Bodde.Query.OData.ODataCriteria;

namespace ODataCriteria;

public class ToString
{
    [Theory]
    [InlineData(null, null, null, null, "")]
    [InlineData("Id eq 5", null, null, null, "$filter=Id eq 5")]
    [InlineData(null, "Id desc", null, null, "$orderby=Id desc")]
    [InlineData(null, null, 1, null, "$skip=1")]
    [InlineData(null, null, null, 3, "$top=3")]
    [InlineData(null, null, 3, 5, "$skip=3&$top=5")]
    [InlineData("Id ge 2", "Id desc", null, null, "$filter=Id ge 2&$orderby=Id desc")]
    [InlineData("Id ge 2", "Id desc", 1, null, "$filter=Id ge 2&$orderby=Id desc&$skip=1")]
    [InlineData("Id ge 2", "Id desc", 1, 3, "$filter=Id ge 2&$orderby=Id desc&$skip=1&$top=3")]
    public void Sunny(string? filter, string? orderby, int? skip, int? top, string expected)
    {
        var sut = new Sut(filter, orderby, skip, top);

        var actual = sut.ToString();

        Assert.Equal(expected, actual);
    }
}
