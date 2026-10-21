using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Auth;
using FluentAssertions;

namespace Core.Tests.Integration;

public class AuditLogsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuditLogsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();

        var register = new RegisterCommand
        {
            Username = $"audit_{Guid.NewGuid():N}".Substring(0, 20),
            Email = $"audit_{Guid.NewGuid():N}@example.com",
            Password = "Password123"
        };

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", register);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);

        return client;
    }

    [Fact]
    public async Task GetAuditLogs_ShouldReturnUnauthorized_WithoutToken()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auditlogs");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAuditLogs_ShouldReturnForbidden_ForNonAdminUser()
    {
        var client = await GetAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/auditlogs");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAuditLogs_ShouldReturnForbidden_ForRegularUserFilteredRequest()
    {
        var client = await GetAuthenticatedClientAsync();

        var response = await client.GetAsync(
            "/api/v1/auditlogs?entityType=Customer&pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
