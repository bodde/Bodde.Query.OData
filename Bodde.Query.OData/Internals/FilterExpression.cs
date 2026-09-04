namespace Bodde.Query.OData.Internals;

/// <summary>
/// Represents a filter expression.
/// </summary>
internal abstract record FilterExpression;

/// <summary>
/// Represents a comparison between a property and a value.
/// </summary>
/// <param name="PropertyPath">The dotted path of the property to compare.</param>
/// <param name="Operator">The comparison operator.</param>
/// <param name="Value">The value to compare with.</param>
internal record ComparisonExpression(string PropertyPath, ComparisonOperator Operator, object? Value) : FilterExpression;

/// <summary>
/// Represents the negation of a filter expression.
/// </summary>
/// <param name="Expression">The expression to negate.</param>
internal record NotExpression(FilterExpression Expression) : FilterExpression;

/// <summary>
/// Represents a logical combination of filter expressions.
/// </summary>
/// <param name="Operator">The logical operator.</param>
/// <param name="First">The first expression.</param>
/// <param name="Second">The second expression.</param>
/// <param name="Others">Additional expressions to combine.</param>
internal record LogicalExpression(LogicalOperator Operator, FilterExpression First, FilterExpression Second, params FilterExpression[] Others) : FilterExpression
{
    /// <summary>
    /// Gets all expressions in this logical expression, in declaration order.
    /// </summary>
    internal FilterExpression[] AllExpressions
    {
        get
        {
            FilterExpression[] expressions =
            [
                First,
                    Second,
                    ..Others
            ];

            return expressions;
        }
    }
}

/// <summary>
/// Specifies the comparison operation used by a comparison expression.
/// </summary>
internal enum ComparisonOperator
{
    /// <summary>Tests whether two values are equal.</summary>
    Equals,
    /// <summary>Tests whether two values are not equal.</summary>
    NotEquals,
    /// <summary>Tests whether the first value is greater than the second.</summary>
    GreaterThan,
    /// <summary>Tests whether the first value is less than the second.</summary>
    LessThan,
    /// <summary>Tests whether the first value is greater than or equal to the second.</summary>
    GreaterThanOrEqual,
    /// <summary>Tests whether the first value is less than or equal to the second.</summary>
    LessThanOrEqual,
    /// <summary>Tests whether a string contains a value.</summary>
    Contains,
    /// <summary>Tests whether a string starts with a value.</summary>
    StartsWith,
    /// <summary>Tests whether a string ends with a value.</summary>
    EndsWith,
    /// <summary>Tests whether a value belongs to a collection of values.</summary>
    In,
}

/// <summary>
/// Specifies the logical operation used to combine filter expressions.
/// </summary>
internal enum LogicalOperator
{
    /// <summary>Requires all combined expressions to match.</summary>
    And,
    /// <summary>Requires at least one combined expression to match.</summary>
    Or
}

