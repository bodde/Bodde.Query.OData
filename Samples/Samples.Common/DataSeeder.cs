namespace Samples.Common;

public static class DataSeeder
{
    public static IEnumerable<Department> SeedDepartments()
    {
        var id = 1;
        return [
          new Department { Id = id++, Name = "Human Resources" },
          new Department { Id = id++, Name = "Engineering" },
          new Department { Id = id++, Name = "Marketing" }
        ];
    }

    public static IEnumerable<Employee> SeedEmployees(IEnumerable<Department> departments)
    {
        var departmentsArray = departments.ToArray();

        string[] firstNames = ["John", "Jane", "Bob", "Alice", "Charlie", "David"];
        string[] lastNames = ["Doe", "Smith", "Johnson", "Williams", "Brown", "Jones"];

        var departmentIndex = 0;
        var id = 1;
        var hireDate = new DateTime(2020, 1, 1);

        foreach(var firstName in firstNames)
        {
            foreach(var lastName in lastNames)
            {
                var department = departmentsArray[departmentIndex];
                departmentIndex = (departmentIndex + 1) % departmentsArray.Length;
                hireDate = hireDate.AddMonths(1);

                yield return new Employee
                {
                    Id = id++,
                    Name = $"{firstName} {lastName}",
                    DepartmentId = department.Id,
                    Department = department,
                    HireDate = hireDate,
                    Salary = 50000 - (id * 1000),
                    IsActive = id % 5 != 0
                };
            }
        }
    }

}
