namespace Bodde.Query.OData.Test.Models;

public interface IIdentifiable<T>
{
    public T Id { get; set; }
}