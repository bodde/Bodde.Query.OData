using Samples.Common;
using Samples.Common.EFCore;
using Microsoft.EntityFrameworkCore;


using var ctx = new CompanyDbContext();

Console.WriteLine($"Initializing database {CompanyDbContext.DbPath}");
await ctx.EnsureRecreatedAsync();

var testExecutor = new ODataQueryTester(ctx.Employees);
await testExecutor.ExecuteAsync(
    toArrayAsync: query => query.ToArrayAsync(), 
    countAsync: query => query.CountAsync(), 
    output: text => Console.WriteLine(text)
    );