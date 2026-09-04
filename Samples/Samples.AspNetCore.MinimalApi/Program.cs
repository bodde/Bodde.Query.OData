using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Samples.Common.EFCore;
using Bodde.Query.OData;
using Samples.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CompanyDbContext>();

builder.Services.AddOpenApi();
builder.Services.AddHostedService<InitializeDatabaseService>();

var app = builder.Build();

MapOpenApi(app);

app.MapGet("/employees", async ([AsParameters]ODataCriteria criteria, CompanyDbContext context) => 
{
    try
    {       
        var items = await context.Employees
            .Include(_ => _.Department) // use dto mapping/projection for real projects
            .Apply(criteria)
            .ToArrayAsync();

        var totalCount = items.Length;
        if(criteria.Top.HasValue || criteria.Skip.HasValue)
        {
            var countCriteria = new ODataCriteria(Filter: criteria.Filter);

            totalCount = await context.Employees
                .Apply(countCriteria)
                .CountAsync();
        }

        var result = new ODataResult<Employee>(items, totalCount);

        return Results.Ok(result);
    }
    catch(FormatException formatEx)
    {
        // query parsing gone wrong
        return Results.BadRequest(formatEx.Message);
    }
    catch(Exception ex)
    {
        app.Logger.LogError(ex, ex.Message);
        return Results.InternalServerError();
    }
});

app.Run();

static void MapOpenApi(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(opt => opt
            .WithClassicLayout()
            .HideSearch()
            .HideSidebar()
            .HideModels()
        );

        // redirect home to scalar
        app
            .MapGet("/", () => Results.Redirect("scalar"))
            .ExcludeFromDescription();
    }
}

internal class InitializeDatabaseService(
    IServiceScopeFactory serviceScopeFactory, 
    ILogger<InitializeDatabaseService> logger
    ) : BackgroundService
{
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Initializing database {dbPath}", CompanyDbContext.DbPath);

        using var scope = serviceScopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<CompanyDbContext>();

        await ctx.EnsureRecreatedAsync(stoppingToken);
    }
}
