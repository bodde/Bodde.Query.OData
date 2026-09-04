using Samples.Common.EFCore;
using Microsoft.AspNetCore.Mvc;
using Samples.Common;
using Microsoft.EntityFrameworkCore;
using Bodde.Query.OData;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CompanyDbContext>();

builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<InitializeDatabaseService>();

builder.Services.AddControllers();

var app = builder.Build();

MapOpenApi(app);

app.MapControllers();

app.Run();

static void MapOpenApi(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        // redirect home to Swagger UI
        app
            .MapGet("/", () => Results.Redirect("swagger"))
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


[ApiController]
[Route("employees")]
public class EmployeesController(CompanyDbContext context) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<ODataResult<Employee>>> Get([FromQuery] ODataCriteria criteria)
	{
		var items = await context.Employees
			.Include(employee => employee.Department) // use dto mapping/projection for real projects
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

		return Ok(result);
	}
}