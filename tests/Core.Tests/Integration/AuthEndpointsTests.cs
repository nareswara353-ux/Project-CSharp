using System.Net;
using System.Net.Http.Json;
using Application.Auth;
using FluentAssertions;

namespace Core.Tests.Integration;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturnCreated_WithValidData()
    {
        var command = new RegisterCommand
        {
            Username = $"user_{Guid.NewGuid():N}".Substring(0, 20),
            Email = $"user_{Guid.NewGuid():N}@example.com",
            Password = "Password123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        auth!.Token.Should().NotBeNullOrWhiteSpace();
        auth.Username.Should().Be(command.Username);
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenUsernameAlreadyExists()
    {
        var username = $"dup_{Guid.NewGuid():N}".Substring(0, 20);
        var first = new RegisterCommand
        {
            Username = username,
            Email = $"first_{Guid.NewGuid():N}@example.com",
            Password = "Password123"
        };

        await _client.PostAsJsonAsync("/api/auth/register", first);

        var second = new RegisterCommand
        {
            Username = username,
            Email = $"second_{Guid.NewGuid():N}@example.com",
            Password = "Password123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", second);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenCredentialsValid()
    {
        var username = $"login_{Guid.NewGuid():N}".Substring(0, 20);
        var registerCommand = new RegisterCommand
        {
            Username = username,
            Email = $"login_{Guid.NewGuid():N}@example.com",
            Password = "Password123"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        var loginCommand = new LoginCommand
        {
            UsernameOrEmail = username,
            Password = "Password123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        auth!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordWrong()
    {
        var username = $"wrong_{Guid.NewGuid():N}".Substring(0, 20);
        var registerCommand = new RegisterCommand
        {
            Username = username,
            Email = $"wrong_{Guid.NewGuid():N}@example.com",
            Password = "Password123"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        var loginCommand = new LoginCommand
        {
            UsernameOrEmail = username,
            Password = "WrongPass999"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
