using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace PocketBaseCore.Tests;

public class PocketBaseSharpClientTests
{
    private ITestOutputHelper _outputHelper { get; }
    private IConfiguration _configuration { get; }
    private readonly IPocketSharpClient _client;
    
    public PocketBaseSharpClientTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true).Build();
        
        //ie. _client = new PocketSharpClient("https://localhost:8090");
        _client = new PocketSharpClient(_configuration["PocketBaseUri"]);
        _client.JsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }
    
    [Fact]
    public async Task AuthenticateAsync_Should_Return_Valid_Response()
    {
        // Arrange
        var identity = _configuration["PocketBaseIdentity"];
        var password = _configuration["PocketBasePassword"];
        
        // Act
        var authResponse = await _client.AuthenticateAsync(identity, password);
        
        // Assert
        Assert.NotNull(authResponse);
        Assert.NotNull(authResponse.Token);
        Assert.NotNull(authResponse.User);
    }
    
    [Fact]
    public async Task AuthenticateAsync_Should_Return_ThrowException_On_Invalid_Credentials()
    {
        // Arrange
        var identity = "wrongIdentity";
        var password = "wrongPassword";
        
        // Act, Assert
        AuthResponse authResponse = null;
        PocketSharpException exception = await Assert.ThrowsAsync<PocketSharpException>(async () =>
        {
            authResponse = await _client.AuthenticateAsync(identity, password);
        });

        // Assert
        Assert.NotNull(exception);
        Assert.NotNull(exception.Message);
        Assert.Null(authResponse);
    }
    
    [Fact]
    public async Task Create_Update_Retrieve_And_Delete_RecordAsync_Simple()
    {
        // Arrange
        await Authenticate();
        
        var company = new Company()
        {
            Name = "Acme Inc",
            Website = "https://acme.com",
            Sector = "Technology"
        };
        
        // Act
        // Create company
        company = await _client.CreateRecordAsync<Company>("company", company);
        
        // Update company (partial update)
        var updateData = new Dictionary<string, object>
        {
            {"name", "Wikipedia"},
            {"website", "https://wikipedia.org"},
            {"sector", "Information"}
        };

        company = await _client.UpdateRecordAsync<Company>("company", company.Id, updateData);
        
        var retrievedCompany = await _client.GetRecordAsync<Company>("company", company.Id);
        // Assert
        Assert.NotNull(company);
        Assert.Equal(company.Name, updateData["name"]);
        Assert.NotNull(retrievedCompany);
        Assert.Equal(retrievedCompany.Name, updateData["name"]);
        
        // Cleanup
        await _client.DeleteRecordAsync("company", company.Id);
    }
    
    [Fact]
    public async Task Create_And_Delete_RecordAsync_JsonNode()
    {
        // Arrange
        await Authenticate();
        JsonNode company = new JsonObject
        {
            ["name"] = "Acme Inc",
            ["website"] = "https://acme.com",
            ["sector"] = "Technology"
        };
        
        // Act
        company = await _client.CreateRecordAsync<JsonNode>("company", company);
        
        // Assert
        Assert.NotNull(company);
        Assert.NotNull(company["id"]?.GetValue<string>());
        
        // Cleanup
        await _client.DeleteRecordAsync("company", company["id"]?.GetValue<string>());
    }

    [Fact]
    public async Task Create_And_Delete_Record_With_Relationship()
    {
        // Arrange
        await Authenticate();
        // Create a company
        var company = new Company()
        {
            Name = "Acme Inc",
            Website = "https://acme.com",
            Sector = "Technology"
        };
        company = await _client.CreateRecordAsync<Company>("company", company);
        
        // Create the record
        var employee = new Employee()
        {
            FullName = "John Doe",
            Email = "johndoe@acme.com",
            PhoneNumber = "1234567890",
            Company = company.Id
        };
        
        employee = await _client.CreateRecordAsync<Employee>("employee", employee, expand: "company");
        
        // Assert
        Assert.NotNull(employee);
        Assert.NotNull(employee.Company);
        Assert.NotNull(employee.Expand.Company);
        Assert.Equal(employee.Expand.Company.Name, company.Name);
        
        // Cleanup
        await _client.DeleteRecordAsync("employee", employee.Id);
        await _client.DeleteRecordAsync("company", company.Id);
    }

    [Fact]
    public async Task Filter_And_Sort_Collection()
    {
        // Arrange
        await Authenticate();
        
        // Generate 10 companies with different names
        var companies = new List<Company>();
        for (var i = 0; i < 10; i++)
        {
            companies.Add(new Company()
            {
                Name = $"Company {i}",
                Website = $"https://company{i}.com",
                Sector = "Technology"
            });
        }
        
        // Create companies
        var results = new List<Company>();
        if (results == null) throw new ArgumentNullException(nameof(results));
        foreach (var company in companies)
        {
            await _client.CreateRecordAsync<Company>("company", company);
        }
        
        // Act
        var companiesSorted = await _client.GetRecordsAsync<Company>("company", fields: "id,name, sector", sort: "-name");  
        var companiesFiltered = await _client.GetRecordsAsync<Company>("company", filter: "(name = 'Company 5')");
        
        // Assert sorted companies
        Assert.NotNull(companiesSorted);
        // 10 companies are found
        Assert.Equal(10, companiesSorted.Items.Count);
        // first company is Company 9
        Assert.Equal("Company 9", companiesSorted.Items.First().Name);
        
        // Assert filtered companies
        Assert.NotNull(companiesFiltered);
        // one company is found for Company 5
        Assert.Single((IEnumerable) companiesFiltered.Items);
        Assert.Equal("Company 5", companiesFiltered.Items.First().Name);
        
        
        // Cleanup
        foreach (var company in companiesSorted.Items)
        {
            await _client.DeleteRecordAsync("company", company.Id);
        }
    }
    
    private async Task<AuthResponse> Authenticate()
    {
        var identity = _configuration["PocketBaseIdentity"];
        var password = _configuration["PocketBasePassword"];
        
        return await _client.AuthenticateAsync(identity, password);
    }
}

public class Employee : PocketBaseRecord
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Company { get; set; }
    public ExpandCompany Expand { get; set; }
}

public class ExpandCompany
{
    public Company Company { get; set; }
}

public class Company : PocketBaseRecord
{
    public string Name { get; set; }
    public string Website { get; set; }
    public string Sector { get; set; }
}