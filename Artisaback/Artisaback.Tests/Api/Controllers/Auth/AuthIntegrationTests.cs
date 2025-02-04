using System.Net;
using System.Net.Http.Json;
using Artisaback.Domain.IRepositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Artisaback.Tests.Api.Controllers.Auth;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Register_Should_Return_Created_With_JwtToken()
    {
        // Arrange
        var email = $"test+{Guid.NewGuid()}@example.com";
        var payload = new { Email = email, Password = "P@ssw0rd", Role = "Customer" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<TokenResponse>();
        content.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Register_With_Admin_Role_Should_Return_Error()
    {
        // Arrange
        var email = $"admin+{Guid.NewGuid()}@example.com";
        var payload = new { Email = email, Password = "P@ssw0rd", Role = "Admin" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        content.Error.Should().Contain("Registration with role 'Admin' is not allowed.");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Login_Should_Return_Ok_With_JwtToken()
    {
        // Arrange
        var email = $"login+{Guid.NewGuid()}@example.com";
        var registerPayload = new { Email = email, Password = "P@ssw0rd", Role = "Customer" };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerPayload);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginPayload = new { Email = email, Password = "P@ssw0rd" };

        // Act
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        content.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RefreshToken_Should_Return_Ok_With_New_JwtToken()
    {
        // Arrange
        var email = $"refresh+{Guid.NewGuid()}@example.com";
        var registerPayload = new { Email = email, Password = "P@ssw0rd", Role = "Customer" };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerPayload);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Récupération du refresh token depuis la db via la factory.
        using (var scope = _factory.Services.CreateScope())
        {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var user = await userRepository.GetByEmailAsync(email);
            user.Should().NotBeNull();
            user.RefreshToken.Should().NotBeNullOrEmpty();

            var refreshPayload = new { user.RefreshToken };

            // Act
            var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh-token", refreshPayload);

            // Assert
            refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await refreshResponse.Content.ReadFromJsonAsync<TokenResponse>();
            content.Token.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Register_With_Duplicate_Email_Should_Return_Error()
    {
        // Arrange
        var email = $"duplicate+{Guid.NewGuid()}@example.com";
        var payload = new { Email = email, Password = "P@ssw0rd", Role = "Customer" };

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/auth/register", payload);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act 
        var response2 = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Unauthenticated_Access_To_Admin_Endpoint_Should_Return_Unauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Unauthenticated_Access_To_Artisan_Endpoint_Should_Return_Unauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/artisans/1/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public record ErrorResponse(string Error);
}