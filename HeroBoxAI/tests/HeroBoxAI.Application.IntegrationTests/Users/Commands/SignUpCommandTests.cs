using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HeroBoxAI.Application.Users;
using HeroBoxAI.Application.Users.Commands.SignUp;
using HeroBoxAI.Domain.Enums;
using HeroBoxAI.WebApi.Endpoints;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HeroBoxAI.Application.IntegrationTests.Users.Commands;

public class SignUpCommandTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SignUpCommandTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesUserAndReturnsAuthResponse()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var command = new SignUpCommand
        {
            Username = $"testuser_{uniqueId}",
            Email = $"test_{uniqueId}@example.com",
            Password = "TestPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignUp, command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
        Assert.NotNull(result.User);
        Assert.Equal(command.Username, result.User.Username);
        Assert.Equal(command.Email, result.User.Email);
        Assert.Equal(UserRole.Player, result.User.Role);
        Assert.Equal(UserStatus.Active, result.User.Status);

        // Verify user was persisted to database
        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HeroBoxAI.Infrastructure.Data.HeroBoxDbContext>();
        var persistedUser = await dbContext.Users.FindAsync(result.User.Id);
        Assert.NotNull(persistedUser);
        Assert.Equal(command.Username, persistedUser.Username);
        Assert.Equal(command.Email, persistedUser.Email);
        Assert.NotEqual(command.Password, persistedUser.PasswordHash); // Password should be hashed
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var firstCommand = new SignUpCommand
        {
            Username = $"user1_{uniqueId}",
            Email = $"duplicate_{uniqueId}@example.com",
            Password = "TestPassword123!"
        };

        var secondCommand = new SignUpCommand
        {
            Username = $"user2_{uniqueId}",
            Email = $"duplicate_{uniqueId}@example.com", // Same email
            Password = "TestPassword123!"
        };

        // Act
        var firstResponse = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignUp, firstCommand);
        var secondResponse = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignUp, secondCommand);

        // Assert
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Handle_DuplicateUsername_ReturnsConflict()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var firstCommand = new SignUpCommand
        {
            Username = $"duplicateuser_{uniqueId}",
            Email = $"user1_{uniqueId}@example.com",
            Password = "TestPassword123!"
        };

        var secondCommand = new SignUpCommand
        {
            Username = $"duplicateuser_{uniqueId}", // Same username
            Email = $"user2_{uniqueId}@example.com",
            Password = "TestPassword123!"
        };

        // Act
        var firstResponse = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignUp, firstCommand);
        var secondResponse = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignUp, secondCommand);

        // Assert
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var command = new SignUpCommand
        {
            Username = $"testuser_{uniqueId}",
            Email = "invalid-email", // Invalid email format
            Password = "TestPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignUp, command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Handle_EmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var command = new SignUpCommand
        {
            Username = "", // Empty username
            Email = $"test_{uniqueId}@example.com",
            Password = "TestPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignUp, command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Handle_WeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var command = new SignUpCommand
        {
            Username = $"testuser_{uniqueId}",
            Email = $"test_{uniqueId}@example.com",
            Password = "123" // Weak password
        };

        // Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Auth.SignUp, command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
} 