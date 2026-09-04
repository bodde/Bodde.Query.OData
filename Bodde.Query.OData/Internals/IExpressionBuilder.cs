using System.Linq.Expressions;

namespace Bodde.Query.OData.Internals;

internal interface IExpressionBuilder
{
    Expression<Func<T, object?>> BuildSelector<T>(string propertyPath);

    Expression<Func<T, bool>> BuildPredicate<T>(FilterExpression filterExpression);

}
