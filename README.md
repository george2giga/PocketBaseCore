# PocketBaseCore

A .NET Core and Standard 2.0 wrapper for the PocketBase API.

## Installation

Install the package via NuGet:

```sh
dotnet add package PocketBaseCore --version 1.0.0
```

## Usage

### Authentication

Before making any requests, authenticate with the API:

```csharp
var client = new PocketBaseClient("https://your-pocketbase-url");
var authResponse = await client.AuthenticateAsync("your-identity", "your-password");
```

### Creating Records

To create a record using a custom POCO class:

```csharp
var company = new Company
{
    Name = "Acme Inc",
    Website = "https://acme.com",
    Sector = "Technology"
};

company = await client.CreateRecordAsync<Company>("company", company);
```

If you prefer not to use a custom POCO class, you can use `JsonNode` instead:

```csharp
var company = new JsonObject
{
    ["name"] = "Acme Inc",
    ["website"] = "https://acme.com",
    ["sector"] = "Technology"
};

company = await client.CreateRecordAsync<JsonNode>("company", company);
```

### Updating a Record

To update a record:

```csharp
var updateData = new Dictionary<string, object>
{
    { "name", "New Acme Inc" }
};

company = await client.UpdateRecordAsync<Company>("company", company.Id, updateData);
```

Alternatively, using `JsonNode`:

```csharp
var updateData = new JsonObject
{
    ["name"] = "New Acme Inc"
};

company = await client.UpdateRecordAsync<JsonNode>("company", company["id"].ToString(), updateData);
```

### Retrieving a Record

To retrieve a record:

```csharp
var retrievedCompany = await client.GetRecordAsync<Company>("company", company.Id);
```

Or with `JsonNode`:

```csharp
var retrievedCompany = await client.GetRecordAsync<JsonNode>("company", company["id"].ToString());
```

### Working with Relationships

To create records with relationships:

```csharp
// Create a company
var company = new Company
{
    Name = "Acme Inc",
    Website = "https://acme.com",
    Sector = "Technology"
};
company = await client.CreateRecordAsync<Company>("company", company);

// Create an employee
var employee = new Employee
{
    FullName = "John Doe",
    Email = "johndoe@acme.com",
    PhoneNumber = "1234567890",
    Company = company.Id
};

employee = await client.CreateRecordAsync<Employee>("employee", employee, expand: "company");
```

Or using `JsonNode` for both:

```csharp
// Create a company
var company = new JsonObject
{
    ["name"] = "Acme Inc",
    ["website"] = "https://acme.com",
    ["sector"] = "Technology"
};
company = await client.CreateRecordAsync<JsonNode>("company", company);

// Create an employee
var employee = new JsonObject
{
    ["fullName"] = "John Doe",
    ["email"] = "johndoe@acme.com",
    ["phoneNumber"] = "1234567890",
    ["company"] = company["id"].ToString()
};

employee = await client.CreateRecordAsync<JsonNode>("employee", employee, expand: "company");
```

### Filtering and Sorting

To filter and sort a collection:

```csharp
var companiesSorted = await client.GetRecordsAsync<Company>("company", fields: "id,name,sector", sort: "-name");
var companiesFiltered = await client.GetRecordsAsync<Company>("company", filter: "(name = 'Company 5')");
```

Or using `JsonNode`:

```csharp
var companiesSorted = await client.GetRecordsAsync<JsonNode>("company", fields: "id,name,sector", sort: "-name");
var companiesFiltered = await client.GetRecordsAsync<JsonNode>("company", filter: "(name = 'Company 5')");
```

### Deleting Records

To delete records, use the following code:

```csharp
// Delete a company
await client.DeleteRecordAsync("company", company.Id);

// Delete an employee
await client.DeleteRecordAsync("employee", employee.Id);
```

If using `JsonNode`:

```csharp
// Delete a company
await client.DeleteRecordAsync("company", company["id"].ToString());

// Delete an employee
await client.DeleteRecordAsync("employee", employee["id"].ToString());
```

## License

This project is licensed under the MIT License.
