using Bodde.Common.Extensions;

namespace Bodde.Query.OData;

/// <summary>
/// Represents the criteria for an OData query, including filter, orderby, skip, and top parameters.
/// </summary>
/// <param name="Select">The OData select string.</param>
/// <param name="Filter">The OData filter string.</param>
/// <param name="OrderBy">The OData orderby string.</param>
/// <param name="Skip">The number of items to skip.</param>
/// <param name="Top">The maximum number of items to return.</param>
public record ODataSelectCriteria(
    string Select,
    string? Filter = null, 
    string? OrderBy = null, 
    int? Skip = null,
    int? Top = null
) : ODataCriteria(Filter, OrderBy, Skip, Top)
{
    public override string ToString()
    {
        var tokens = base.ToStringTokens();
        tokens.Insert(0, String.Concat("$select=", Select));

        return tokens.ToCsv("&");
    }
}
