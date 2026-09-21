using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Auth;
using Application.Customers;
using Application.Orders;
using FluentAssertions;

namespace Core.Tests.Integration;

public class OrdersEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OrdersEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();

        var registerCommand = new RegisterCommand
        {
            Username = $"ord_{Guid.NewGuid():N}".Substring(0, 20),
            Email = $"ord_{Guid.NewGuid():N}@example.com",
            Password = "Password123"
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", registerCommand);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);

        return client;
    }

    private static async Task<Guid> CreateCustomerAsync(HttpClient client)
    {
        var command = new CreateCustomerCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = $"customer_{Guid.NewGuid():N}@example.com",
            Street = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA"
        };

        var response = await client.PostAsJsonAsync("/api/customers", command);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static CreateOrderCommand BuildOrderCommand(Guid customerId) => new()
    {
        CustomerId = customerId,
        Notes = "Integration test order",
        Lines = new List<OrderLineRequest>
        {
            new()
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Test Widget",
                UnitPrice = 25.50m,
                Currency = "USD",
                Quantity = 2
            }
        }
    };

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
        var client = await GetAuthenticatedClientAsync();
        var customerId = await CreateCustomerAsync(client);
        var command = BuildOrderCommand(customerId);

        var response = await client.PostAsJsonAsync("/api/orders", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ShouldReturnNotFound_WhenCustomerMissing()
    {
        var client = await GetAuthenticatedClientAsync();
        var command = BuildOrderCommand(Guid.NewGuid());

        var response = await client.PostAsJsonAsync("/api/orders", command);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenOrderExists()
    {
        var client = await GetAuthenticatedClientAsync();
        var customerId = await CreateCustomerAsync(client);
        var command = BuildOrderCommand(customerId);

        var createResponse = await client.PostAsJsonAsync("/api/orders", command);
        var orderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var getResponse = await client.GetAsync($"/api/orders/{orderId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await getResponse.Content.ReadFromJsonAsync<OrderDto>();
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(orderId);
        dto.Status.Should().Be("Draft");
        dto.Lines.Should().HaveCount(1);
        dto.TotalAmount.Should().Be(51.00m);
    }

    [Fact]
    public async Task Confirm_ShouldReturnNoContent_WhenOrderHasLines()
    {
        var client = await GetAuthenticatedClientAsync();
        var customerId = await CreateCustomerAsync(client);
        var command = BuildOrderCommand(customerId);

        var createResponse = await client.PostAsJsonAsync("/api/orders", command);
        var orderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var confirmResponse = await client.PostAsync($"/api/orders/{orderId}/confirm", null);

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/orders/{orderId}");
        var dto = await getResponse.Content.ReadFromJsonAsync<OrderDto>();
        dto!.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task Ship_ShouldReturnNoContent_WhenOrderConfirmed()
    {
        var client = await GetAuthenticatedClientAsync();
        var customerId = await CreateCustomerAsync(client);
        var command = BuildOrderCommand(customerId);

        var createResponse = await client.PostAsJsonAsync("/api/orders", command);
        var orderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        await client.PostAsync($"/api/orders/{orderId}/confirm", null);
        var shipResponse = await client.PostAsync($"/api/orders/{orderId}/ship", null);

        shipResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Ship_ShouldReturnBadRequest_WhenOrderNotConfirmed()
    {
        var client = await GetAuthenticatedClientAsync();
        var customerId = await CreateCustomerAsync(client);
        var command = BuildOrderCommand(customerId);

        var createResponse = await client.PostAsJsonAsync("/api/orders", command);
        var orderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var shipResponse = await client.PostAsync($"/api/orders/{orderId}/ship", null);

        shipResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cancel_ShouldReturnNoContent_WhenDraft()
    {
        var client = await GetAuthenticatedClientAsync();
        var customerId = await CreateCustomerAsync(client);
        var command = BuildOrderCommand(customerId);

        var createResponse = await client.PostAsJsonAsync("/api/orders", command);
        var orderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var cancelResponse = await client.PostAsync($"/api/orders/{orderId}/cancel", null);

        cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
