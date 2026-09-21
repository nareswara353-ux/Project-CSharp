using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Auth;
using Application.Customers;
using FluentAssertions;

namespace Core.Tests.Integration;

public class CustomersEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CustomersEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();

        var registerCommand = new RegisterCommand
        {
            Username = $"cust_{Guid.NewGuid():N}".Substring(0, 20),
            Email = $"cust_{Guid.NewGuid():N}@example.com",
            Password = "Password123"
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", registerCommand);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);

        return client;
    }

    private static CreateCustomerCommand BuildCommand(string suffix) => new()
    {
        FirstName = "John",
        LastName = $"Doe{suffix}",
        Email = $"john_{Guid.NewGuid():N}@example.com",
        Street = "123 Main St",
        City = "New York",
        State = "NY",
        PostalCode = "10001",
        Country = "USA"
    };

    [Fact]
    public async Task Create_ShouldReturnCreated_WithValidData()
    {
        var client = await GetAuthenticatedClientAsync();
        var command = BuildCommand("A");

        var response = await client.PostAsJsonAsync("/api/customers", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WithoutToken()
    {
        var client = _factory.CreateClient();
        var command = BuildCommand("B");

        var response = await client.PostAsJsonAsync("/api/customers", command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenCustomerExists()
    {
        var client = await GetAuthenticatedClientAsync();
        var command = BuildCommand("C");

        var createResponse = await client.PostAsJsonAsync("/api/customers", command);
        var customerId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var getResponse = await client.GetAsync($"/api/customers/{customerId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await getResponse.Content.ReadFromJsonAsync<CustomerDto>();
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(customerId);
        dto.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        var client = await GetAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/customers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_ShouldReturnPagedResults()
    {
        var client = await GetAuthenticatedClientAsync();

        await client.PostAsJsonAsync("/api/customers", BuildCommand("D1"));
        await client.PostAsJsonAsync("/api/customers", BuildCommand("D2"));

        var response = await client.GetAsync("/api/customers?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenValid()
    {
        var client = await GetAuthenticatedClientAsync();
        var createCommand = BuildCommand("E");

        var createResponse = await client.PostAsJsonAsync("/api/customers", createCommand);
        var customerId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var updateCommand = new UpdateCustomerCommand
        {
            Id = customerId,
            FirstName = "Jane",
            LastName = "Smith",
            Email = $"jane_{Guid.NewGuid():N}@example.com",
            Street = "456 Oak Ave",
            City = "Los Angeles",
            State = "CA",
            PostalCode = "90210",
            Country = "USA"
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/customers/{customerId}", updateCommand);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/customers/{customerId}");
        var dto = await getResponse.Content.ReadFromJsonAsync<CustomerDto>();
        dto!.FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenCustomerExists()
    {
        var client = await GetAuthenticatedClientAsync();
        var command = BuildCommand("F");

        var createResponse = await client.PostAsJsonAsync("/api/customers", command);
        var customerId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var deleteResponse = await client.DeleteAsync($"/api/customers/{customerId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/customers/{customerId}");
        var dto = await getResponse.Content.ReadFromJsonAsync<CustomerDto>();
        dto!.IsActive.Should().BeFalse();
    }
}
