using Samples.Common;

var departments = DataSeeder.SeedDepartments();
var employees = DataSeeder.SeedEmployees(departments);

var testExecutor = new ODataQueryTester(employees.AsQueryable());

testExecutor.Execute(
    toArray: query => query.ToArray(), 
    count: query => query.Count(), 
    output: text => Console.WriteLine(text)
    );
