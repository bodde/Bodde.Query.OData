using System.Linq.Expressions;

namespace Bodde.Query.OData;

/// <summary>
/// Defines a contract for parsing OData filter and orderby strings into LINQ expressions.
/// </summary>
public interface IODataParser
{
    /// <summary>
    /// Parses the given OData filter string into a LINQ expression.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="filterString">The OData filter string.</param>
    /// <returns>The parsed LINQ expression.</returns>
    Expression<Func<T, bool>> ParseFilter<T>(string filterString);
    
    /// <summary>
    /// Parses the given OData orderby string into a sequence of LINQ expressions.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="orderByString">The OData orderby string.</param>
    /// <returns>The parsed LINQ expressions.</returns>
    OrderByExpression<T>[] ParseOrderBy<T>(string orderByString);
}
