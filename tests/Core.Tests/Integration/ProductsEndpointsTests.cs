using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Auth;
using Application.Products;
using FluentAssertions;

namespace Core.Tests.Integration;

public class ProductsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProductsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();

        var registerCommand = new RegisterCommand
        {
            Username = $"prod_{Guid.NewGuid():N}".Substring(0, 20),
            Email = $"prod_{Guid.NewGuid():N}@example.com",
            Password = "Password123"
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", registerCommand);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);

        return client;
    }

    private static CreateProductCommand BuildCommand(string sku) => new()
    {
        Name = "Widget",
        Description = "A fine widget for testing",
        Sku = sku,
        Price = 99.99m,
        Currency = "USD",
        StockQuantity = 50
    };

    [Fact]
    public async Task Create_ShouldReturnCreated_WithValidData()
    {
        var client = await GetAuthenticatedClientAsync();
        var command = BuildCommand($"PRD-{Guid.NewGuid():N}".Substring(0, 15));

        var response = await client.PostAsJsonAsync("/api/products", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WithoutToken()
    {
        var client = _factory.CreateClient();
        var command = BuildCommand($"PRD-{Guid.NewGuid():N}".Substring(0, 15));

        var response = await client.PostAsJsonAsync("/api/products", command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenProductExists()
    {
        var client = await GetAuthenticatedClientAsync();
        var command = BuildCommand($"PRD-{Guid.NewGuid():N}".Substring(0, 15));

        var createResponse = await client.PostAsJsonAsync("/api/products", command);
        var productId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var getResponse = await client.GetAsync($"/api/products/{productId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(productId);
        dto.Name.Should().Be("Widget");
        dto.Price.Should().Be(99.99m);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var client = await GetAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_ShouldReturnPagedResults()
    {
        var client = await GetAuthenticatedClientAsync();

        await client.PostAsJsonAsync("/api/products", BuildCommand($"PRD-{Guid.NewGuid():N}".Substring(0, 15)));
        await client.PostAsJsonAsync("/api/products", BuildCommand($"PRD-{Guid.NewGuid():N}".Substring(0, 15)));

        var response = await client.GetAsync("/api/products?pageNumber=1&pageSize=10&isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenValid()
    {
        var client = await GetAuthenticatedClientAsync();
        var command = BuildCommand($"PRD-{Guid.NewGuid():N}".Substring(0, 15));

        var createResponse = await client.PostAsJsonAsync("/api/products", command);
        var productId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var updateCommand = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Widget",
            Description = "Updated description",
            Price = 149.99m,
            Currency = "USD",
            StockQuantity = 25
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/products/{productId}", updateCommand);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/products/{productId}");
        var dto = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        dto!.Name.Should().Be("Updated Widget");
        dto.Price.Should().Be(149.99m);
    }
}
