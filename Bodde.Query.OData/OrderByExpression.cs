using System.Linq.Expressions;

namespace Bodde.Query.OData;

/// <summary>
/// Represents an order by expression for a specific type.
/// </summary>
/// <typeparam name="TItem">The type of the items to order.</typeparam>
/// <param name="Selector">The expression to select the property to order by.</param>
/// <param name="IsDescending">A value indicating whether to order in descending order.</param>
public record OrderByExpression<TItem>(Expression<Func<TItem, object?>> Selector, bool IsDescending = false);
