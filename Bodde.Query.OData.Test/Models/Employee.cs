namespace Bodde.Query.OData.Test.Models;
public class Employee : IIdentifiable<long>
{
    public long Id { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string Email { get; set; }

    public RoleType Role { get; set; }

    public DateTimeOffset HireDateTimeOffset { get; set; }

    public DateTime HireDate => HireDateTimeOffset.Date;

    public decimal Salary { get; set; }

    public bool IsActive { get; set; }

    public Department? Department { get; set; }

    public enum RoleType
    {
        Developer,
        Manager,
        Analyst,
        Tester
    }
}
