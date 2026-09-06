using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Bodde.Common.Extensions;

namespace Bodde.Query.OData.Internals;

internal class ODataParser(IExpressionBuilder expressionBuilder) : IODataParser
{
    public Expression<Func<T, bool>> ParseFilter<T>(string filterString)
    {
        var parser = new FilterParser();
        var filterCriteria = parser.Parse(filterString);

        var expression = expressionBuilder.BuildPredicate<T>(filterCriteria);

        return expression;
    }

    public OrderByExpression<T>[] ParseOrderBy<T>(string orderByString)
    {
        var parser = new OrderByParser();
        var orderByItems = parser.Parse(orderByString);

        var orderByExpressions = orderByItems
            .Select(_ => new OrderByExpression<T>
            (
                Selector: expressionBuilder.BuildSelector<T>(_.PropertyPath),
                _.IsDescending
            ))
            .ToArray();

        return orderByExpressions;
    }

    internal class FilterParser
    {
        internal FilterExpression Parse(string filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            filter = filter.Trim();

            var expressionsBag = new Dictionary<string, FilterExpression>();

            filter = ProcessComparisonExpressions(filter, expressionsBag);
            filter = ProcessNotAndLogicalExpressions(filter, expressionsBag);

            return CreateTopLevelExpression(filter, expressionsBag);
        }


        private string ProcessComparisonExpressions(string filterString, Dictionary<string, FilterExpression> expressionsBag)
        {
            var comparisonStatements = GetComparisonStatements(filterString);

            foreach (var comparisonStatement in comparisonStatements)
            {
                string expressionKey = NextExpressionKey(expressionsBag);

                expressionsBag[expressionKey] = CreateComparisonExpression(comparisonStatement);

                filterString = filterString.Replace(comparisonStatement, expressionKey);
            }

            return filterString;
        }

        private string ProcessNotAndLogicalExpressions(string filterString, Dictionary<string, FilterExpression> expressionsBag)
        {
            while (true)
            {
                filterString = ProcessNotExpressions(filterString, expressionsBag);

                var innerExpressions = GetInnerLogicalExpressions(filterString);
                if (innerExpressions.IsEmpty())
                    break;

                filterString = ProcessInnerLogicalExpressions(filterString, innerExpressions, expressionsBag);
            }

            return filterString;
        }

        private static string ProcessNotExpressions(string filterString, Dictionary<string, FilterExpression> expressionsBag)
        {
            var notMatches = NotExpressionsRegex.Matches(filterString);
            
            foreach (Match notMatch in notMatches)
            {
                var notStatement = notMatch.Value;

                string innerExpressionKey = GetNotInnerExpressionKey(notMatch.Groups);
                var innerExpression = expressionsBag[innerExpressionKey];

                var notExpression = new NotExpression(innerExpression);

                var notExpressionKey = NextExpressionKey(expressionsBag);
                expressionsBag[notExpressionKey] = notExpression;

                filterString = filterString.Replace(notStatement, notExpressionKey);
            }

            return filterString;
        }

        internal static string GetNotInnerExpressionKey(GroupCollection notMatchGroups)
        {
            var notMatchKey1Group = notMatchGroups["key1"];
            if (notMatchKey1Group != null && notMatchKey1Group.Success)
                return notMatchKey1Group.Value;

            var notMatchKey2Group = notMatchGroups["key2"];
            if (notMatchKey2Group != null && notMatchKey2Group.Success)
                return notMatchKey2Group.Value;

            throw new FormatException("No valid group found for not expression.");
        }

        private static string[] GetInnerLogicalExpressions(string filterString)
            => SurroundedByParenthesesRegex.GetMatchValues(filterString);

        private string ProcessInnerLogicalExpressions(
            string filterString,
            string[] innerExpressions,
            Dictionary<string, FilterExpression> expressionsBag
            )
        {
            foreach (var innerExpression in innerExpressions)
            {
                var expression = innerExpression.Substring(1, innerExpression.Length - 2); // remove surrounding parentheses

                var logicalExpression = CreateLogicalExpression(expression, expressionsBag);

                var logicalExpressionKey = NextExpressionKey(expressionsBag);
                expressionsBag[logicalExpressionKey] = logicalExpression;

                filterString = filterString.Replace(innerExpression, logicalExpressionKey);
            }
            return filterString;
        }

        private FilterExpression CreateTopLevelExpression(string filterString, Dictionary<string, FilterExpression> expressionsBag)
        {
            if (LogicalOperatorsRegex.IsMatch(filterString))
            {
                return CreateLogicalExpression(filterString, expressionsBag);
            }

            if (expressionsBag.ContainsKey(filterString))
                return expressionsBag[filterString];

            throw new FormatException("Unable to create top-level expression from filter string.");
        }

        private static string NextExpressionKey(Dictionary<string, FilterExpression> expressionsBag)
        {
            return $"|{expressionsBag.Count}|";
        }

        private LogicalExpression CreateLogicalExpression(
            string expressionString,
            Dictionary<string, FilterExpression> expressionsBag
            )
        {
            var logicalOperator = GetLogicalOperator(expressionString);
            var expressionKeys = GetExpressionKeys(expressionString);

            if (expressionKeys.Length < 2)
                throw new FormatException("At least two expressions are required to create a logical expression.");

            var expressions = expressionKeys
                .Select(key => expressionsBag[key])
                .ToArray();

            var logicalExpression = new LogicalExpression(
                Operator: logicalOperator,
                First: expressions[0],
                Second: expressions[1],
                Others: expressions.Length > 2 ? expressions.Skip(2).ToArray() : Array.Empty<FilterExpression>()
                );

            return logicalExpression;
        }
        private string[] GetExpressionKeys(string expressionString)
            =>  ExpressionKeysRegex.GetMatchValues(expressionString);

        private LogicalOperator GetLogicalOperator(string filterString)
        {
            var logicalOperators = LogicalOperatorsRegex
                .GetMatchValues(filterString)
                .Select(v => v.Trim().ToLower())
                .ToArray();

            if (logicalOperators.Distinct().Count() > 1)
            {
                throw new FormatException("Only one logical operator per logical expression is supported.");
            }

            var logicalOperator = ConvertLogicalOperatorStringToEnum(logicalOperators[0]);
            return logicalOperator;
        }

        internal static LogicalOperator ConvertLogicalOperatorStringToEnum(string value)
        {
            return value switch
            {
                "and" => LogicalOperator.And,
                "or" => LogicalOperator.Or,
                _ => throw new FormatException($"Logical operator '{value}' is not supported.")
            };
        }

        internal static ComparisonExpression CreateComparisonExpression(string comparisonStatement)
        {
            var parts = ExpressionRegex.GetMatchValues(comparisonStatement.Trim());

            if (parts.Length != 3)
            {
                throw new FormatException("Binary comparison expressions only are supported.");
            }

            var propertyPath = parts[0];
            var operatorString = parts[1];
            var valueString = parts[2];

            var comparisonOperator = ConvertODataOperatorToComparisonOperator(operatorString);
            var value = ConvertValueString(valueString, comparisonOperator);

            var comparisonExpression = new ComparisonExpression(
                PropertyPath: propertyPath,
                Operator: comparisonOperator,
                Value: value
            );
            return comparisonExpression;
        }

        public string[] GetComparisonStatements(string filterString)
        {
            var comparisonStatements = ComparisonStatementsRegex.GetMatchValues(filterString);

            if (comparisonStatements.IsEmpty())
            {
                throw new FormatException("No valid comparison statements found in filter string.");
            }

            return comparisonStatements;
        }

        private static object? ConvertValueString(string valueString, ComparisonOperator comparisonOperator)
        {
            if (comparisonOperator == ComparisonOperator.In)
            {
                // handle 'in' operator with multiple values in parentheses
                var match = InValuesRegex.Match(valueString);
                if (!match.Success)
                {
                    throw new FormatException("Invalid syntax for 'in' operator.");
                }

                var valuesPart = match.Groups[1].Value;
                var valueTypes = valuesPart.Tokenize(',', trim: true)
                    .Select(v => ParseValue(v))
                    .ToArray();

                var types = valueTypes.Select(vt => vt.type).ToArray();
                if (types.Distinct().Count() > 1)
                {
                    throw new FormatException("All values for 'in' operator must be of the same type.");
                }

                // convert to array of the appropriate type
                var elementType = valueTypes.First().type;
                var arrayType = elementType.MakeArrayType();
                var values = Array.CreateInstance(elementType, valueTypes.Length);
                for (int i = 0; i < valueTypes.Length; i++)
                {
                    values.SetValue(valueTypes[i].value, i);
                }

                return values;
            }

            return ParseValue(valueString).value;
        }

        internal static (object? value, Type type) ParseValue(string valueString)
        {
            if (valueString.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return (null!, typeof(object));
            }

            var quotesRegex = QuotesRegex;
            if (quotesRegex.IsMatch(valueString))
            {
                return (quotesRegex.Replace(valueString, "$1"), typeof(string));
            }

            if (int.TryParse(valueString, out var intValue))
            {
                return (intValue, typeof(int));
            }
            if (double.TryParse(valueString, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
            {
                return (doubleValue, typeof(double));
            }
            if (bool.TryParse(valueString, out var boolValue))
            {
                return (boolValue, typeof(bool));
            }
            if (DateTime.TryParse(
                valueString,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var dateTimeValue
                ))
            {
                return (dateTimeValue, typeof(DateTime));
            }

            throw new NotImplementedException($"Unable to parse value string '{valueString}'.");
        }

        private static ComparisonOperator ConvertODataOperatorToComparisonOperator(string operatorString)
        {
            if (_comparisonOperators.TryGetValue(operatorString, out var comparisonOperator))
            {
                return comparisonOperator;
            }

            throw new FormatException($"OData operator '{operatorString}' is not supported. Supported operators are: {_comparisonOperators.Keys.ToCsv()}.");
        }

        private static readonly Dictionary<string, ComparisonOperator> _comparisonOperators = new()
        {
            {"eq", ComparisonOperator.Equals},
            {"ne", ComparisonOperator.NotEquals},
            {"gt", ComparisonOperator.GreaterThan},
            {"ge", ComparisonOperator.GreaterThanOrEqual},
            {"lt", ComparisonOperator.LessThan},
            {"le", ComparisonOperator.LessThanOrEqual},
            {"contains", ComparisonOperator.Contains},
            {"startswith", ComparisonOperator.StartsWith},
            {"endswith", ComparisonOperator.EndsWith},
            {"in", ComparisonOperator.In}
        };

        private static readonly Regex ComparisonStatementsRegex = new(
            @"([\w\.]+\s+(?:\w+)\s+(?:null|true|false|'[^']*'|[\d\-T\:\.Z]+|\((?:(?:null|true|false|'[^']*'|[\d\-T\:\.Z]+)(?:\s*,\s*)?)+\)))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex LogicalOperatorsRegex = new(
            @"\s+(and|or)\s+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal static readonly Regex NotExpressionsRegex = new(
            @"(?:not\s*\((?'key1'\|\d+\|)\))|(?:not\s*(?'key2'\|\d+\|))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SurroundedByParenthesesRegex = new(
            @"\(([^()]+)\)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ExpressionRegex = new(
            @"('([^']|'')*'|\([^)]*\)|\S+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex QuotesRegex = new(
            @"^'(.*)'$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ExpressionKeysRegex = new(
            @"(\|\d+\|)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex InValuesRegex = new(
            @"\((.*)\)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    }

    internal class OrderByParser
    {

        public OrderByItem[] Parse(string orderByString)
        {
            if (orderByString == null)
                throw new ArgumentNullException(nameof(orderByString));

            var orderByItems = orderByString.FromCsv(ParseOrderByItem, removeEmpty: false);

            return orderByItems;
        }

        private OrderByItem ParseOrderByItem(string itemString)
        {
            var tokens = itemString.Tokenize(' ');

            var propertyPath = tokens.ElementAtOrDefault(0);
            var descending = tokens.ElementAtOrDefault(1);
            var isDescending = descending is null
                ? false
                : descending.Equals("desc", StringComparison.OrdinalIgnoreCase);

            return new OrderByItem(
                PropertyPath: propertyPath,
                IsDescending: isDescending
            );
        }
    }

    internal record OrderByItem(string PropertyPath, bool IsDescending);

}