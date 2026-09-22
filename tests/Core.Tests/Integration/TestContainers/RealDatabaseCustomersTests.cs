using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Auth;
using Application.Customers;
using FluentAssertions;

namespace Core.Tests.Integration.TestContainers;

[Collection("SqlServerCollection")]
public class RealDatabaseCustomersTests
{
    private readonly SqlServerFixture _sqlFixture;

    public RealDatabaseCustomersTests(SqlServerFixture sqlFixture)
    {
        _sqlFixture = sqlFixture;
    }

    private async Task<HttpClient> GetAuthenticatedClientAsync(RealDatabaseWebApplicationFactory factory)
    {
        var client = factory.CreateClient();

        var register = new RegisterCommand
        {
            Username = $"real_{Guid.NewGuid():N}".Substring(0, 20),
            Email = $"real_{Guid.NewGuid():N}@example.com",
            Password = "Password123"
        };

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", register);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);

        return client;
    }

    [Fact]
    public async Task CreateCustomer_ShouldPersist_ToRealDatabase()
    {
        await using var factory = new RealDatabaseWebApplicationFactory(_sqlFixture.ConnectionString);
        var client = await GetAuthenticatedClientAsync(factory);

        var command = new CreateCustomerCommand
        {
            FirstName = "Real",
            LastName = "Database",
            Email = $"real_{Guid.NewGuid():N}@example.com",
            Street = "123 Real St",
            City = "Real City",
            State = "RC",
            PostalCode = "12345",
            Country = "USA"
        };

        var createResponse = await client.PostAsJsonAsync("/api/v1/customers", command);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var customerId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var getResponse = await client.GetAsync($"/api/v1/customers/{customerId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await getResponse.Content.ReadFromJsonAsync<CustomerDto>();
        dto!.FirstName.Should().Be("Real");
        dto.LastName.Should().Be("Database");
    }
}
