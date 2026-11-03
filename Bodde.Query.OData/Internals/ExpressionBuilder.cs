using System.Collections;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using Bodde.Common.Extensions;

namespace Bodde.Query.OData.Internals;

internal class ExpressionBuilder() : IExpressionBuilder
{

    public Expression<Func<T, object?>> BuildSelector<T>(string propertyPath)
    {
        var parameter = Expression.Parameter(typeof(T)); ;
        var selectorBuilder = new SelectorBuilder<T>(parameter);

        return selectorBuilder.BuildExpression(propertyPath);
    }

    public Expression<Func<T, bool>> BuildPredicate<T>(FilterExpression filterExpression)
    {
        var parameter = Expression.Parameter(typeof(T));
        var predicateBuilder = new PredicateBuilder<T>(parameter);

        return predicateBuilder.BuildExpression(filterExpression);
    }

    public Expression<Func<T, dynamic>> BuildSelectedType<T>(SelectItem[] selectItems)
    {
        var parameter = Expression.Parameter(typeof(T));
        var selectBuilder = new SelectorBuilder<T>(parameter);

        return selectBuilder.BuildExpression(selectItems);
    }

    internal abstract class ExpressionBuilderBase(ParameterExpression parameter)
    {
        protected ParameterExpression Parameter => parameter;


        protected MemberExpression CreateMemberExpressionFromPath(string propertyPath)
        {
            var properties = propertyPath.Tokenize('.');
            if(properties.Length == 0)
            {
                throw new InvalidOperationException("Property path cannot be empty.");
            }   
            var firstPropertyName = properties.First();
            var nextPropertyNames = properties.Skip(1).ToArray();

            var currentExpression = Expression.PropertyOrField(Parameter, firstPropertyName);
            foreach (var nextPropertyName in nextPropertyNames)
            {
                currentExpression = Expression.PropertyOrField(currentExpression, nextPropertyName);
            }

            return currentExpression;
        }
    }

    internal class SelectorBuilder<T>(ParameterExpression parameter) : ExpressionBuilderBase(parameter)
    {
        internal Expression<Func<T, object?>> BuildExpression(string propertyPath)
        {
            var propertyPathExpression = CreateMemberExpressionFromPath(propertyPath);
            var converted = Expression.Convert(propertyPathExpression, typeof(object));

            return Expression.Lambda<Func<T, object?>>(converted, Parameter);
        }

        internal Expression<Func<T, dynamic>> BuildExpression(SelectItem[] selectItems)
        {
            var propertyExpressions = selectItems
                .Select(selectItem => CreateMemberExpressionFromPath(selectItem.PropertyPath))
                .ToArray();

            // expression that creates expando object with the selected properties
            var newExpandoExpression = Expression.New(typeof(ExpandoObject));
            var addPropertyMethod = typeof(IDictionary<string, object>).GetMethod("Add")!;
            var initPropertyExpressions = propertyExpressions
                .Select(propertyExpression => CreateAddPropertyExpression(propertyExpression, addPropertyMethod))
                .ToArray();

            var initExpandoExpression = Expression.ListInit(newExpandoExpression, initPropertyExpressions);
            var convertedExpandoExpression = Expression.Convert(initExpandoExpression, typeof(object));

            return Expression.Lambda<Func<T, dynamic>>(convertedExpandoExpression, Parameter);
        }


        private static ElementInit CreateAddPropertyExpression(MemberExpression pe, MethodInfo addPropertyMethod)
        {
            return Expression.ElementInit(
                                addPropertyMethod,
                                Expression.Constant(pe.Member.Name),
                                Expression.Convert(pe, typeof(object))
                            );
        }
    }

    internal class PredicateBuilder<T>(ParameterExpression parameter) : ExpressionBuilderBase(parameter)
    {
        public Expression<Func<T, bool>> BuildExpression(FilterExpression filterExpression)
        {
            if (filterExpression == null)
                throw new ArgumentNullException(nameof(filterExpression));


            return filterExpression switch
            {
                LogicalExpression logicalExpression => CreateLogicalExpression(logicalExpression),
                ComparisonExpression comparisonExpression => CreateComparisonExpression(comparisonExpression),
                NotExpression notExpression => CreateNotExpression(notExpression),

                _ => throw new NotImplementedException($"Filter expression type {filterExpression.GetType().Name} is not implemented.")
            };
        }

        internal Expression<Func<T, bool>> CreateLogicalExpression(LogicalExpression logicalExpression)
        {
            if (logicalExpression.First == null || logicalExpression.Second == null)
            {
                throw new InvalidOperationException("At least two expressions must be provided for a logical expression.");
            }

            Expression? combinedBody = null;
            foreach (var expression in logicalExpression.AllExpressions)
            {
                var expressionLambda = BuildExpression(expression);

                combinedBody = combinedBody == null
                    ? expressionLambda.Body
                    : logicalExpression.Operator switch
                    {
                        LogicalOperator.And => Expression.AndAlso(combinedBody, expressionLambda.Body),
                        LogicalOperator.Or => Expression.OrElse(combinedBody, expressionLambda.Body),
                        _ => throw new InvalidOperationException($"Logical operator {logicalExpression.Operator} is not supported.")
                    };
            }

            var lambda = Expression.Lambda<Func<T, bool>>(combinedBody!, Parameter);

            return lambda;
        }

        private Expression<Func<T, bool>> CreateComparisonExpression(ComparisonExpression comparisonExpression)
        {
            var property = CreateMemberExpressionFromPath(comparisonExpression.PropertyPath);
            var constant = CreateConstantExpression(comparisonExpression, property);

            ValidateOperatorOrThrow(comparisonExpression, property);

            var operatorExpression = CreateOperatorExpression(comparisonExpression, property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(operatorExpression, Parameter);

            return lambda;
        }

        private Expression<Func<T, bool>> CreateNotExpression(NotExpression notExpression)
        {
            var expressionLambda = BuildExpression(notExpression.Expression);
            var lambda = Expression.Lambda<Func<T, bool>>(Expression.Not(expressionLambda.Body), Parameter);

            return lambda;
        }

        private static ConstantExpression CreateConstantExpression(ComparisonExpression comparisonExpression, Expression property)
        {
            var value = GetValueFromExpression(comparisonExpression, property);

            var constantType = CreateConstantType(comparisonExpression, property);

            return Expression.Constant(value, constantType);
        }

        private static Type CreateConstantType(ComparisonExpression comparisonExpression, Expression property)
        {
            return comparisonExpression.Operator == ComparisonOperator.In
                ? typeof(IEnumerable<>).MakeGenericType(property.Type)
                : property.Type;
        }

        internal static void ValidateOperatorOrThrow(ComparisonExpression comparisonExpression, Expression property)
        {
            bool isStringOperator = HasStringOperator(comparisonExpression);

            if (isStringOperator && property.Type != typeof(string))
            {
                throw new InvalidOperationException($"Operator {comparisonExpression.Operator} can only be applied to string properties.");
            }
        }

        private static bool HasStringOperator(ComparisonExpression comparisonExpression)
        {
            var stringExpressions = new[]
            {
            ComparisonOperator.Contains,
            ComparisonOperator.StartsWith,
            ComparisonOperator.EndsWith
        };

            var isStringOperator = stringExpressions.Contains(comparisonExpression.Operator);
            return isStringOperator;
        }

        internal Expression CreateOperatorExpression(ComparisonExpression comparisonExpression, Expression property, ConstantExpression constant)
        {
            return comparisonExpression.Operator switch
            {
                ComparisonOperator.Equals => Expression.Equal(property, constant),
                ComparisonOperator.NotEquals => Expression.NotEqual(property, constant),
                ComparisonOperator.GreaterThan => Expression.GreaterThan(property, constant),
                ComparisonOperator.LessThan => Expression.LessThan(property, constant),
                ComparisonOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, constant),
                ComparisonOperator.LessThanOrEqual => Expression.LessThanOrEqual(property, constant),
                ComparisonOperator.Contains => Expression.Call(property, GetStringMethodInfo(nameof(string.Contains)), constant),
                ComparisonOperator.StartsWith => Expression.Call(property, GetStringMethodInfo(nameof(string.StartsWith)), constant),
                ComparisonOperator.EndsWith => Expression.Call(property, GetStringMethodInfo(nameof(string.EndsWith)), constant),
                ComparisonOperator.In => Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [property.Type], constant, property),

                _ => throw new NotSupportedException($"Operator {comparisonExpression.Operator} is not supported.")
            };
        }

        internal static object? GetValueFromExpression(ComparisonExpression comparisonExpression, Expression property)
        {
            var value = comparisonExpression.Value;

            if (value == null)
                return null;

            if (comparisonExpression.Operator == ComparisonOperator.In)
            {
                return GetInValues(property, value);
            }

            return ConvertConstantValueToPropertyType(value, property.Type);
        }

        internal static object? ConvertConstantValueToPropertyType(object value, Type propertyType)
        {
            var valueType = value.GetType();
            if (valueType == propertyType)
                return value;

            if (value is DateTime dateTimeValue && propertyType == typeof(DateTimeOffset))
            {
                return new DateTimeOffset(dateTimeValue);
            }

            if (value is DateTimeOffset dateTimeOffsetValue && propertyType == typeof(DateTime))
            {
                return dateTimeOffsetValue.DateTime;
            }

            if (value is string stringValue)
            {
                return stringValue.ConvertTo(propertyType);
            }

            var needsConversion = valueType != propertyType;
            if (needsConversion)
            {
                value = Convert.ChangeType(value, propertyType);
            }

            return value;
        }

        internal static object GetInValues(Expression property, object value)
        {
            var valueType = value.GetType();
            if (!valueType.IsCollection())
            {
                throw new InvalidOperationException("Value for 'In' operator must be a collection.");
            }

            var elementType = valueType.GetElementType() ?? valueType.GetGenericArguments().FirstOrDefault();
            if (elementType == property.Type)
                return value;

            var listInstance = ConvertInValues(value, property.Type);

            return listInstance;
        }

        internal static IList ConvertInValues(object value, Type propertyType)
        {
            if (value is not IEnumerable enumerableValue)
            {
                throw new ArgumentException("Value for 'In' operator must be a IEnumerable collection.");
            }

            var listType = typeof(List<>).MakeGenericType(propertyType);
            var listInstance = (IList)Activator.CreateInstance(listType)!;

            foreach (var elementValue in enumerableValue)
            {
                var convertedItem = ConvertConstantValueToPropertyType(elementValue, propertyType);
                listInstance.Add(convertedItem);
            }

            return listInstance;
        }

        private MethodInfo GetStringMethodInfo(string methodName)
        {
            var arguments = new[] { typeof(string) };
            return typeof(string).GetMethod(methodName, arguments)!;
        }
    }
}


