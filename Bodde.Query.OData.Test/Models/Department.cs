namespace Bodde.Query.OData.Test.Models;

public class Department : IIdentifiable<int>
{
    public int Id { get; set; }

    public required string Name { get; set; }
}
