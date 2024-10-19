using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using PocketBaseCore;


namespace PocketBaseCore.Example
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("PocketSharp Example");

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<PocketSharpClient>();

            IPocketSharpClient client = new PocketSharpClient("https://pbase.instance.capocean.online", logger);
            client.JsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            // Authenticate
            var authResponse = await client.AuthenticateAsync("spidee", "spidee01");
            Console.WriteLine($"Authenticated User: {authResponse.User.Email}");
            
            // Create a company
            var company = new Company()
            {
                Name = "Acme Inc",
                Website = "https://acme.com",
                Sector = "Technology"
            };
            
            company = await client.CreateRecordAsync<Company>("company", company);
            Console.WriteLine($"Created strongly typed company ID: {company.Id}");

            // Create the record
            var employee = new Employee()
            {
                FullName = "John Doe",
                Email = "johndoe@acme.com",
                PhoneNumber = "1234567890",
                Company = company.Id
            };
            
            employee = await client.CreateRecordAsync<Employee>("employee", employee);
            Console.WriteLine($"Created strongly typed employee ID: {employee.Id}");
            
            // Create a JsonNode record, more flexible than dynamic
            JsonNode jsonNodeEmployee = new JsonObject()
            {
                ["fullName"] = "Jack Doe",
                ["email"] = "johndoe@acme.com",
                ["phoneNumber"] = "1234567890",
                ["company"] = company.Id
            };
            
            jsonNodeEmployee = await client.CreateRecordAsync<JsonNode>("employee", jsonNodeEmployee);
            Console.WriteLine($"Created jsonNodeEmployee Record ID: {jsonNodeEmployee["id"]?.GetValue<string>()}");;

            // Fetch the record
            employee = await client.GetRecordAsync<Employee>("employee", jsonNodeEmployee["Id"]?.GetValue<string>());
            Console.WriteLine($"Fetched Record jsonNode: Title={employee.FullName}");
        
            // Update the record (partial update)
            var updateData = new Dictionary<string, object>
            {
                {"fullName", "Clark Kent"},
                {"email", "clark_kent@email.com"}
            };

            var updatedRecord = await client.UpdateRecordAsync<Employee>(
                "employee",
                employee.Id,
                updateData);

            Console.WriteLine($"Updated Record Value: {updatedRecord.FullName}");

            // Fetch multiple records
            var recordsList = await client.GetRecordsAsync<Employee>(
                "employee",
                filter: "fullName = 'Clark Kent'",
                sort: "-created");

            Console.WriteLine($"Records List: {recordsList.TotalItems}");
            foreach (var record in recordsList.Items)
            {
                Console.WriteLine($"ID: {record.Id}, FullName: {record.FullName}, Email: {record.Email}");
            }

            // Delete the record
            await client.DeleteRecordAsync("employee", employee.Id);
            await client.DeleteRecordAsync("employee", jsonNodeEmployee["id"]?.GetValue<string>());
            Console.WriteLine("Record deleted successfully.");
        }
    }
}

public class Employee : PocketBaseRecord
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Company { get; set; }
}

public class Company : PocketBaseRecord
{
    public string Name { get; set; }
    public string Website { get; set; }
    public string Sector { get; set; }
}