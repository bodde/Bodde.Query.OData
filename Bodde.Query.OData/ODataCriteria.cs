using Bodde.Common.Extensions;

namespace Bodde.Query.OData;

/// <summary>
/// Represents the criteria for an OData query, including filter, orderby, skip, and top parameters.
/// </summary>
/// <param name="Filter">The OData filter string.</param>
/// <param name="OrderBy">The OData orderby string.</param>
/// <param name="Skip">The number of items to skip.</param>
/// <param name="Top">The maximum number of items to return.</param>
public record ODataCriteria(
    string? Filter = null, 
    string? OrderBy = null, 
    int? Skip = null,
    int? Top = null
)
{
    public override string ToString()
    {
        var tokens = new List<string>();
        if(Filter is not null) tokens.Add(String.Concat("$filter=", Filter));
        if(OrderBy is not null) tokens.Add(String.Concat("$orderby=", OrderBy));
        if(Skip is not null) tokens.Add(String.Concat("$skip=", Skip.ToString()));
        if(Top is not null) tokens.Add(String.Concat("$top=", Top.ToString()));

        return tokens.ToCsv("&");
    }
}
