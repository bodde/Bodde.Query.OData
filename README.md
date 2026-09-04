# Bodde.Query.OData

Bodde.Query.OData is a NuGet package for building, parsing, formatting, and executing query criteria with OData syntax. 
It provides reusable support for filtering, sorting, and paging `IQueryable<T>` data sources.  
It supports a lightweight partial, limited subset of OData query constructs, including `$filter`, `$orderby`, `$top`, `$skip`.  
These constructs can describe queries without requiring a dependency on `Microsoft.OData` or the needing EDM models generation.


## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [API reference](#api-reference)
- [Example projects with source code](#example-projects-with-source-code)
- [Limitations](#limitations)
- [Configuration](#configuration)
- [Compatibility](#compatibility)
- [Versioning](#versioning)
- [Contributing](#contributing)
- [License](#license)

## Features

- Lightweight, partial support for OData-style `$filter`, `$orderby`, `$top`, `$skip` query constructs.
- Comparison operators including `eq`, `ne`, `gt`, `ge`, `lt`, `le`, `contains`, `startswith`, `endswith`, and `in`.
- Logical filter composition with `and`, `or`, `not`, and parenthesized expressions.
- Can be used to compose Entity Framework Core queries.
- Can be used with ASP.NET Core Minimal APIs and MVC controllers.
- No dependency on `Microsoft.OData` or generated EDM models.
- Parser can be replaced/extended with custom implementations.

## Installation

Install the following package:

```bash
dotnet add package Bodde.Query.OData
```

## Usage

The examples below assume an `IQueryable<Employee>` named `employees`:

### 1. Apply criteria

```csharp
var result = employees.Apply(new()).ToArray();

Console.WriteLine($"Returned: {result.Length}");
```

`Apply` returns an `IQueryable<T>`, so the query is not executed until a terminal operation such as `ToArray`, `ToList`, or `Count` is called.

### 2. Filter results

```csharp
var criteria = new ODataCriteria(Filter: "IsActive eq true");
var result = employees.Apply(criteria).ToArray();

Console.WriteLine($"Returned: {result.Length}");
```

### 3. Filter and sort results

```csharp
var criteria = new ODataCriteria(
    Filter: "IsActive eq true",
    OrderBy: "HireDateTimeOffset desc");
var result = employees.Apply(criteria).ToArray();

Console.WriteLine($"Returned: {result.Length}");
```

Multiple ordering expressions can be separated by commas, for example `"Department.Name desc, Id"`.

### 4. Apply paging

```csharp
var criteria = new ODataCriteria(
    Filter: "Department.Name eq 'Engineering'",
    OrderBy: "HireDateTimeOffset desc",
    Skip: 20,
    Top: 10);
var result = employees.Apply(criteria).ToArray();

Console.WriteLine($"Returned: {result.Length}");
```

To calculate the total number of filtered items, apply the filter without paging and execute `Count` separately:

```csharp
var filteredCriteria = new ODataCriteria(Filter: criteria.Filter);
var totalCount = employees.Apply(filteredCriteria).Count();
```

### 5. Apply a complete set of criteria

```csharp
var criteria = new ODataCriteria(
    Filter: "Role in ('Developer', 'Analyst') and IsActive eq true",
    OrderBy: "Department.Name, Id desc",
    Skip: 3,
    Top: 5);

var result = employees.Apply(criteria).ToArray();

Console.WriteLine($"Returned: {result.Length}");
```

The optional `IODataParser` argument can be supplied to `Apply` when a custom parser is required:

```csharp
var result = employees.Apply(criteria, customParser).ToArray();
```

### 6. Entity Framework Core with paging and total count

`Apply` composes directly with an Entity Framework Core `IQueryable`. Execute the paged query first, then run a second query with the same filter and without `Skip` and `Top` to obtain the total count:

```csharp
var criteria = new ODataCriteria(
	Filter: "IsActive eq true",
	OrderBy: "HireDateTimeOffset desc",
	Skip: 20,
	Top: 10);

var items = await context.Employees
	.Include(employee => employee.Department)
	.Apply(criteria)
	.ToArrayAsync();

var countCriteria = new ODataCriteria(Filter: criteria.Filter);
var totalCount = await context.Employees
	.Apply(countCriteria)
	.CountAsync();
```

### 7. Minimal API with Entity Framework Core, paging and total count

In a Minimal API, bind the query parameters directly to `ODataCriteria` and apply them to the EF Core query:

```csharp
app.MapGet("/employees", async ([AsParameters] ODataCriteria criteria, CompanyDbContext context) =>
{
	var items = await context.Employees
		.Include(employee => employee.Department)
		.Apply(criteria)
		.ToArrayAsync();

	var totalCount = items.Length;
	if (criteria.Skip.HasValue || criteria.Top.HasValue)
	{
		var countCriteria = new ODataCriteria(Filter: criteria.Filter);
		totalCount = await context.Employees
			.Apply(countCriteria)
			.CountAsync();
	}

	return Results.Ok(new { items, totalCount });
});
```

For example, the following request applies a filter, ordering, and paging:

```http
GET /employees?$filter=IsActive%20eq%20true&$orderby=HireDateTimeOffset%20desc&$skip=0&$top=10
```

### 8. ASP.NET Core MVC with Entity Framework Core, paging and total count

In an ASP.NET Core MVC controller, bind `ODataCriteria` from the query string and apply it to the EF Core query:

```csharp
[ApiController]
[Route("employees")]
public class EmployeesController(
	CompanyDbContext context) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult> Get([FromQuery] ODataCriteria criteria)
	{
		var items = await context.Employees
			.Include(employee => employee.Department)
			.Apply(criteria)
			.ToArrayAsync();

		var totalCount = items.Length;
		if (criteria.Skip.HasValue || criteria.Top.HasValue)
		{
			var countCriteria = new ODataCriteria(Filter: criteria.Filter);
			totalCount = await context.Employees
				.Apply(countCriteria)
				.CountAsync();
		}

		return Ok(new { items, totalCount });
	}
}
```

For example, the following request can be handled by the controller:

```http
GET /employees?filter=IsActive%20eq%20true&orderby=HireDateTimeOffset%20desc&skip=0&top=10
```

## API reference

### IQueryable\<T\>.Apply extension method

[`Apply`](Bodde.Query.OData/QueryableExtensions.cs) applies filtering, ordering, skipping, and taking to an `IQueryable<T>`. It returns a new query without executing it. When no parser is supplied, it uses the default parser implementation.

```csharp
public static IQueryable<T> Apply<T>(
	this IQueryable<T> queryable,
	ODataCriteria criteria,
	IODataParser? parser = null);
```

### ODataCriteria record

[`ODataCriteria`](Bodde.Query.OData/ODataCriteria.cs) represents the optional criteria for an OData-style query. Its `Filter`, `OrderBy`, `Skip`, and `Top` properties correspond to the supported query parameters. The record's `ToString` method formats the populated criteria as a query string.

```csharp
public record ODataCriteria(
	string? Filter = null,
	string? OrderBy = null,
	int? Skip = null,
	int? Top = null);
```

```csharp
var criteria = new ODataCriteria(
	Filter: "IsActive eq true",
	OrderBy: "HireDateTimeOffset desc",
	Skip: 20,
	Top: 10);

var queryString = criteria.ToString();
// "$filter=IsActive eq true&$orderby=HireDateTimeOffset desc&$skip=20&$top=10"
```

### IODataParser interface

[`IODataParser`](Bodde.Query.OData/IODataParser.cs) defines the parser contract used by `Apply`. Implementations convert OData-style filter and order-by strings into LINQ expressions.

```csharp
public interface IODataParser
{
	Expression<Func<T, bool>> ParseFilter<T>(string filterString);
	OrderByExpression<T>[] ParseOrderBy<T>(string orderByString);
}
```

### OrderByExpression record

[`OrderByExpression<TItem>`](Bodde.Query.OData/OrderByExpression.cs) represents one ordering instruction. `Selector` identifies the property to sort by, and `IsDescending` specifies whether the ordering is descending.

```csharp
public record OrderByExpression<TItem>(
	Expression<Func<TItem, object?>> Selector,
	bool IsDescending = false);
```

### OData-style filters reference

Bodde.Query supports the following comparison operators:

| Operator | Description | Example |
| --- | --- | --- |
| `eq` | Equal to | `Salary eq 50000` |
| `ne` | Not equal to | `Department.Name ne 'Sales'` |
| `gt` | Greater than | `Salary gt 50000` |
| `ge` | Greater than or equal to | `HireDate ge 2021-01-01` |
| `lt` | Less than | `Salary lt 50000` |
| `le` | Less than or equal to | `Salary le 50000` |
| `contains` | Contains a string | `Name contains 'John'` |
| `startswith` | Starts with a string | `Name startswith 'John'` |
| `endswith` | Ends with a string | `Name endswith 'son'` |
| `in` | Matches one of several values | `Department.Name in ('Sales', 'Engineering')` |

Use `and` to require all expressions to match:

```csharp
var result = employees.Apply(new ODataCriteria(
    Filter: "IsActive eq true and Salary gt 50000"));
```

Use `or` to match at least one expression:

```csharp
var result = employees.Apply(new ODataCriteria(
    Filter: "Department.Name eq 'Sales' or Department.Name eq 'Engineering'"));
```

Use `not` to negate a comparison or a parenthesized expression:

```csharp
var result = employees.Apply(new ODataCriteria(
    Filter: "not (IsActive eq true)"));
```

Logical operators can be combined in nested expressions by using parentheses:

```csharp
var result = employees.Apply(new ODataCriteria(
    Filter: "(IsActive eq true and Salary gt 50000) or (Department.Name eq 'Sales' and HireDate ge 2021-01-01)"));
```

## Example projects with source code

The repository includes sample projects demonstrating different usage scenarios:

| Sample | Description |
| --- | --- |
| [Samples.ConsoleApp](https://github.com/bodde/Bodde.Query.OData/tree/main/Samples/Samples.ConsoleApp) | Runs queries in a console application without dependency injection. |
| [Samples.ConsoleApp.EFCore](https://github.com/bodde/Bodde.Query.OData/tree/main/Samples/Samples.ConsoleApp.EFCore) | Runs queries against an Entity Framework Core database. |
| [Samples.AspNetCore.MinimalApi](https://github.com/bodde/Bodde.Query.OData/tree/main/Samples/Samples.AspNetCore.MinimalApi) | Exposes employee queries through an ASP.NET Core Minimal API. |
| [Samples.AspNetCore.Mvc](https://github.com/bodde/Bodde.Query.OData/tree/main/Samples/Samples.AspNetCore.Mvc) | Exposes employee queries through an ASP.NET Core MVC API. |

## Limitations
The filter parser has limited OData support, and each comparison must be binary. Use parentheses when composing complex criteria.

## Compatibility

The Bodde.Query.OData package targets `netstandard2.0`.

The sample projects follow the runtime they demonstrate: `Samples.Common` and `Samples.Common.EFCore` target both `net8.0` and `net10.0`, while the MVC sample currently targets `net8.0`.

The library works with `IQueryable<T>` and can be used with LINQ providers that support the generated expression trees. When using Entity Framework Core, the final query must also be translatable by the configured database provider.

The OData support is intentionally partial and does not require `Microsoft.OData` or generated EDM models. Supported query constructs and operators are documented in the [Usage](#usage) section.


## Versioning

Bodde.Query.Odata follows [Semantic Versioning](https://semver.org/) using the `MAJOR.MINOR.PATCH` format.

- `MAJOR` versions introduce breaking API or behavior changes.
- `MINOR` versions add backward-compatible features.
- `PATCH` versions include backward-compatible bug fixes and maintenance updates.

Release notes and package versions are published with each release of the repository.

## Contributing

Contributions, bug reports, and feature requests are welcome.

### Development requirements

- .NET SDK 10.0 or later
- Git

### Build and test

Clone the repository, restore its dependencies, build the solution, and run the test suite:

```bash
git clone https://github.com/bodde/Bodde.Query.git
cd Bodde.Query
dotnet restore
dotnet build
dotnet test
```

The solution contains the packages under `Bodde.Query.*`, automated tests under `Bodde.Query.*.Test`, and runnable examples under `Samples`.

### Pull requests

When submitting a pull request:

- Keep changes focused and consistent with the existing project structure.
- Add or update tests for behavioral changes.
- Update the README when changing public APIs, package behavior, or usage.
- Ensure `dotnet build` and `dotnet test` complete successfully.
- Describe the motivation and relevant design decisions in the pull request.

## License

Bodde.Query is released under the [MIT License](LICENSE).

Copyright (c) 2026 Tomaso Donini.