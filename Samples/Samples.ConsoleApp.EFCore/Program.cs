using Samples.Common;
using Samples.Common.EFCore;
using Microsoft.EntityFrameworkCore;
using Bodde.Query.OData;
using Bodde.Common.Extensions;


using var ctx = new CompanyDbContext();

Console.WriteLine($"Initializing database {CompanyDbContext.DbPath}");
await ctx.EnsureRecreatedAsync();

var testExecutor = new ODataQueryTester(ctx.Employees);
await testExecutor.ExecuteAsync(
    toArrayAsync: query => query.ToArrayAsync(), 
    countAsync: query => query.CountAsync(), 
    output: text => Console.WriteLine(text)
    );

var result = ctx.Employees.Apply(new ODataSelectCriteria(
    Select:"Name, Salary", 
    Skip: 0, 
    Top: 10
    )).ToArray();

Console.WriteLine(result.ToCsv(_ => $"{_.Name} ({_.Salary})"));