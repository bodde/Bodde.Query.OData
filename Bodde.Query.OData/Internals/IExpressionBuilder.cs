using System.Linq.Expressions;

namespace Bodde.Query.OData.Internals;

internal interface IExpressionBuilder
{
    Expression<Func<T, bool>> BuildPredicate<T>(FilterExpression filterExpression);

    Expression<Func<T, object?>> BuildSelector<T>(string propertyPath);
    
    Expression<Func<T, dynamic>> BuildSelectedType<T>(SelectItem[] selectItems);
}
