using System.IdentityModel.Tokens.Jwt;
using Artisaback.Application.Services;
using Artisaback.Domain.Entities;
using Artisaback.Domain.IRepositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Artisaback.Tests.Application.Services;

public class AuthServiceTests
{
    private readonly AuthService _authService;
    private readonly IConfiguration _config;
    private readonly Mock<IUserRepository> _userRepositoryMock = new();

    public AuthServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            { "Jwt:Secret", "YourLongerSuperSecretKey1234567890" }
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _authService = new AuthService(_userRepositoryMock.Object, _config);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task RegisterAsync_ShouldReturn_ValidJwtToken()
    {
        // Arrange
        var email = "test@example.com";
        var password = "P@ssw0rd";
        var role = "Customer";

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(email))
            .ReturnsAsync((User)null);
        _userRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var token = await _authService.RegisterAsync(email, password, role);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == email);
        jwtToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == role);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LoginAsync_ShouldReturn_Null_WhenUserNotFound()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "any";

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(email))
            .ReturnsAsync((User)null);

        // Act
        var token = await _authService.LoginAsync(email, password);

        // Assert
        token.Should().BeNull();
    }
}