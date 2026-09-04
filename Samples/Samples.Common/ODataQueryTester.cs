using Bodde.Common.Extensions;
using Bodde.Query.OData;

namespace Samples.Common;

public class ODataQueryTester(IQueryable<Employee> employees)
{

    public static class Filter
    {
        public const string Active = "IsActive eq true";
        public const string NotActive = $"not {Active}";
        public const string NamedJohn = "Name startswith 'John'";
        public const string FromHR = "Department.Name eq 'Human Resources'";   
        public const string FromEngineeringOrHR = "Department.Name in ('Human Resources', 'Engineering')";
        public const string SalaryOver30000 = "Salary gt 30000";
        public const string HiredStartingIn2021 = "HireDate ge 2021-01-01";
    }

    public static class OrderBy
    {            
        public const string Department = "Department.Name";
        public const string DepartmentDesc = $"{Department} desc";
        public const string HireDate = "HireDate";
        public const string HireDateDesc = $"{HireDate} desc";
    }

    private record QueryEntry(string Name, ODataCriteria Criteria);

    private static readonly QueryEntry[] queryEntries = [
        new("Employees - Page 1", new(Skip: 0, Top: 10)),
        new("Employees - Page 2", new(Skip: 10, Top: 10)),
        new("Employees - Page 3", new(Skip: 20, Top: 10)),
        new("Employees - Page 4", new(Skip: 30, Top: 10)),
        new("Active employees", new(Filter: Filter.Active)),
        new("Not active employees", new(Filter: Filter.NotActive)),
        new("Employees named John", new(Filter: Filter.NamedJohn)),
        new("Employees from HR", new(Filter: Filter.FromHR)),
        new("Employees by hire date (descending)", new(OrderBy: OrderBy.HireDateDesc)),
        new("Employees from HR by hire date (descending)", new(Filter.FromHR, OrderBy.HireDateDesc)),
        new("Employees from HR by hire date (descending) - Page 2", new(Filter.FromHR, OrderBy.HireDateDesc, Skip: 10, Top: 10)),
        new("Active employees from HR by hire date (descending)", new($"{Filter.Active} and {Filter.FromHR}", OrderBy.HireDateDesc)),
        new("Not active employees from HR by hire date (descending)", new($"{Filter.NotActive} and {Filter.FromHR}", OrderBy.HireDateDesc)),
        new("Active employees, hired from 2021 onwards, from Engineering or Human Resources or with salary over 30000, ordered by department (descending) and hire date - Page 1", 
            new(
                Filter: $"{Filter.Active} and {Filter.HiredStartingIn2021} and ({Filter.FromEngineeringOrHR} or {Filter.SalaryOver30000})",
                OrderBy: $"{OrderBy.DepartmentDesc}, {OrderBy.HireDate}",
                Skip: 0, Top: 10
            ))
    ];

    public void Execute(Func<IQueryable<Employee>, Employee[]> toArray, Func<IQueryable<Employee>, int> count, Action<string> output)
    {
        foreach (var queryEntry in queryEntries)
        {
            var criteria = queryEntry.Criteria;
            var query = employees.Apply(criteria);
            var items = toArray(query);

            var totalCount = items.Length;
            if(criteria.Top.HasValue || criteria.Skip.HasValue)
            {
                query = employees.Apply(new(criteria.Filter));
                totalCount = count(query);
            }

            var result = new ODataResult<Employee>(items, totalCount);
            Output(queryEntry, result, output);
        }
    }


    public async Task ExecuteAsync(Func<IQueryable<Employee>, Task<Employee[]>> toArrayAsync, Func<IQueryable<Employee>, Task<int>> countAsync,  Action<string> output)
    {
        foreach (var queryEntry in queryEntries)
        {
            var criteria = queryEntry.Criteria;
            var query = employees.Apply(criteria);
            var items = await toArrayAsync(query);

            var totalCount = items.Length;
            if(criteria.Top.HasValue || criteria.Skip.HasValue)
            {
                query = employees.Apply(new(criteria.Filter));
                totalCount = await countAsync(query);
            }

            var result = new ODataResult<Employee>(items, totalCount);
            Output(queryEntry, result, output);
        }
    }

    private static void Output(QueryEntry queryEntry, ODataResult<Employee> result, Action<string> output)
    {        
        output($"{queryEntry.Name}:");
        output($"OData: {queryEntry.Criteria}");

        output(result.Items.FormatAsTable(
            new(_ => _.Id),
            new(_ => _.Name),
            new(_ => _.Department!.Name, header: "Department"),
            new(_ => _.Salary, formatter: value => $"{value:C}"),
            new(_ => _.HireDate, header: "Hire Date", formatter: value => $"{value:yyyy-MM-dd}"),
            new(_ => _.IsActive, header: "Is Active")
            ));

        output($"Total items count: {result.TotalCount}");

        output(string.Empty); // new line
    }
}