using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace Artisaback.Tests.Api.Controllers;

public class RoleBasedAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RoleBasedAuthorizationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task AdminUser_Should_Access_Admin_Endpoints()
    {
        // Arrange : Utilisation de l'email admin seedé (défini dans la config)
        var adminEmail = "admin@example.com";
        var adminPassword = "P@ssw0rd";

        // Authentifier via le login (on suppose que l'admin existe grâce au seeding)
        var loginPayload = new { Email = adminEmail, Password = adminPassword };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        tokenResponse.Token.Should().NotBeNullOrEmpty();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        // Act
        var response = await _client.GetAsync("/api/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ArtisanUser_Should_Access_Artisan_Endpoints()
    {
        // Arrange
        var email = $"artisan+{Guid.NewGuid()}@example.com";
        var payload = new { Email = email, Password = "P@ssw0rd", Role = "Artisan" };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", payload);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var tokenResponse = await registerResponse.Content.ReadFromJsonAsync<TokenResponse>();
        tokenResponse.Token.Should().NotBeNullOrEmpty();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        // Act
        var response = await _client.GetAsync("/api/artisans/1/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CustomerUser_Should_Be_Forbidden_From_Artisan_Endpoints()
    {
        // Arrange
        var email = $"customer+{Guid.NewGuid()}@example.com";
        var payload = new { Email = email, Password = "P@ssw0rd", Role = "Customer" };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", payload);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var tokenResponse = await registerResponse.Content.ReadFromJsonAsync<TokenResponse>();
        tokenResponse.Token.Should().NotBeNullOrEmpty();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        // Act
        var response = await _client.GetAsync("/api/artisans/1/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DeliveryPartnerUser_Should_Access_Delivery_Endpoints()
    {
        // Arrange
        var email = $"delivery+{Guid.NewGuid()}@example.com";
        var payload = new { Email = email, Password = "P@ssw0rd", Role = "DeliveryPartner" };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", payload);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var tokenResponse = await registerResponse.Content.ReadFromJsonAsync<TokenResponse>();
        tokenResponse.Token.Should().NotBeNullOrEmpty();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        // Act
        var response = await _client.GetAsync("/api/delivery/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public record TokenResponse(string Token);