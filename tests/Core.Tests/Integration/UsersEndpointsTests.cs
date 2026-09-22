using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Auth;
using Application.Users;
using FluentAssertions;

namespace Core.Tests.Integration;

public class UsersEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public UsersEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient client, AuthResponse auth)> RegisterAsync()
    {
        var client = _factory.CreateClient();

        var command = new RegisterCommand
        {
            Username = $"usr_{Guid.NewGuid():N}".Substring(0, 20),
            Email = $"usr_{Guid.NewGuid():N}@example.com",
            Password = "Password123"
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", command);
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>())!;

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        return (client, auth);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnOk_WithValidToken()
    {
        var (client, auth) = await RegisterAsync();

        var response = await client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<UserDto>();
        dto.Should().NotBeNull();
        dto!.Username.Should().Be(auth.Username);
        dto.Email.Should().Be(auth.Email);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUnauthorized_WithoutToken()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnNoContent_WhenValid()
    {
        var (client, _) = await RegisterAsync();

        var command = new ChangePasswordCommand
        {
            CurrentPassword = "Password123",
            NewPassword = "NewPassword456"
        };

        var response = await client.PostAsJsonAsync("/api/users/me/change-password", command);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnBadRequest_WhenCurrentPasswordWrong()
    {
        var (client, _) = await RegisterAsync();

        var command = new ChangePasswordCommand
        {
            CurrentPassword = "WrongPassword999",
            NewPassword = "NewPassword456"
        };

        var response = await client.PostAsJsonAsync("/api/users/me/change-password", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
