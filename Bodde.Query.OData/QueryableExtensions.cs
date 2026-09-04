using Bodde.Query.OData.Internals;

namespace Bodde.Query.OData;

/// <summary>
/// Provides extension methods for applying OData criteria to IQueryable sequences.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies the specified OData criteria to the given IQueryable sequence, including filtering, ordering, skipping, and taking elements.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="queryable">The IQueryable sequence to apply the criteria to.</param>
    /// <param name="criteria">The OData criteria to apply.</param>
    /// <param name="parser">An optional IODataParser instance for parsing the criteria. If not provided, a default parser will be used.</param>
    /// <returns>The IQueryable sequence with the criteria applied.</returns>
    public static IQueryable<T> Apply<T>(this IQueryable<T> queryable, ODataCriteria criteria, IODataParser? parser = null)
    {
        parser ??= new ODataParser(new ExpressionBuilder());

        if (criteria.Filter is not null)
        {
            var filterExpression = parser.ParseFilter<T>(criteria.Filter);
            queryable = queryable.Where(filterExpression);
        }

        if (criteria.OrderBy is not null)
        {
            var orderByExpressions = parser.ParseOrderBy<T>(criteria.OrderBy);
            var isFirstOrderByExpression = true;
            foreach (var orderByExpression in orderByExpressions)
            {
                if (isFirstOrderByExpression)
                {
                    queryable = orderByExpression.IsDescending
                        ? queryable.OrderByDescending(orderByExpression.Selector)
                        : queryable.OrderBy(orderByExpression.Selector);

                    isFirstOrderByExpression = false;
                    continue;
                }

                if (queryable is IOrderedQueryable<T> orderedQueryable)
                {
                    queryable = orderByExpression.IsDescending
                        ? orderedQueryable.ThenByDescending(orderByExpression.Selector)
                        : orderedQueryable.ThenBy(orderByExpression.Selector);
                }
            }
        }

        if (criteria.Skip.HasValue)
        {
            queryable = queryable.Skip(criteria.Skip.Value);
        }

        if (criteria.Top.HasValue)
        {
            queryable = queryable.Take(criteria.Top.Value);
        }

        return queryable;
    }
}


