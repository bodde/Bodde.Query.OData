using Microsoft.EntityFrameworkCore;

namespace Samples.Common.EFCore;

public class CompanyDbContext : DbContext
{

    private static string DbDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Bodde.Query");

    public static string DbPath
    {
        get
        {
            Directory.CreateDirectory(DbDirectory);
            return Path.Combine(DbDirectory, "companies.db");
        }
    }

    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }

    public async Task EnsureRecreatedAsync(CancellationToken ct = default)
    {
        await Database.EnsureDeletedAsync(ct);
        await Database.EnsureCreatedAsync(ct);
#if !NET10_0_OR_GREATER
        await SeedDataAsync(this, false, ct);
#endif
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
#if NET10_0_OR_GREATER
        options.UseAsyncSeeding(SeedDataAsync);
#endif
    }

    private async Task SeedDataAsync(DbContext ctx, bool _, CancellationToken ct)
    {
        var departments = DataSeeder.SeedDepartments();
        await ctx.Set<Department>().AddRangeAsync(departments, ct);

        var employees = DataSeeder.SeedEmployees(departments);
        await ctx.Set<Employee>().AddRangeAsync(employees, ct);

        await ctx.SaveChangesAsync(ct);
    }
}
