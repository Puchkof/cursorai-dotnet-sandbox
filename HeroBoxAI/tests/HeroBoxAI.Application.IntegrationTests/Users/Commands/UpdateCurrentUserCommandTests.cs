using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HeroBoxAI.Application.Users;
using HeroBoxAI.Application.Users.Commands.UpdateCurrentUser;
using HeroBoxAI.Domain.Entities;
using HeroBoxAI.Domain.Enums;
using HeroBoxAI.WebApi.Endpoints;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HeroBoxAI.Application.IntegrationTests.Users.Commands;

public class UpdateCurrentUserCommandTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public UpdateCurrentUserCommandTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ValidUpdate_UpdatesUserAndReturnsUserDto()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        
        var command = new UpdateCurrentUserCommand
        {
            Username = $"updated_{uniqueId}",
            Email = $"updated_{uniqueId}@example.com"
        };

        // Act
        var response = await client.PutAsJsonAsync(ApiRoutes.Users.Me, command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(command.Username, result.Username);
        Assert.Equal(command.Email, result.Email);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task Handle_NoAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var command = new UpdateCurrentUserCommand
        {
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var response = await client.PutAsJsonAsync(ApiRoutes.Users.Me, command);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Handle_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");
        
        var command = new UpdateCurrentUserCommand
        {
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var response = await client.PutAsJsonAsync(ApiRoutes.Users.Me, command);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Handle_EmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        var command = new UpdateCurrentUserCommand
        {
            Username = "", // Empty username
            Email = "test@example.com"
        };

        // Act
        var response = await client.PutAsJsonAsync(ApiRoutes.Users.Me, command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Handle_EmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        var command = new UpdateCurrentUserCommand
        {
            Username = "testuser",
            Email = "" // Empty email
        };

        // Act
        var response = await client.PutAsJsonAsync(ApiRoutes.Users.Me, command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        var command = new UpdateCurrentUserCommand
        {
            Username = "testuser",
            Email = "invalid-email" // Invalid email format
        };

        // Act
        var response = await client.PutAsJsonAsync(ApiRoutes.Users.Me, command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Handle_DuplicateUsername_ReturnsConflict()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        
        // Create first user
        var (client1, user1) = await _fixture.CreateAuthenticatedClientWithUserAsync(
            username: $"existinguser_{uniqueId}");
        
        // Create second user
        var (client2, user2) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        var command = new UpdateCurrentUserCommand
        {
            Username = $"existinguser_{uniqueId}", // Same username as first user
            Email = $"newemail_{uniqueId}@example.com"
        };

        // Act
        var response = await client2.PutAsJsonAsync(ApiRoutes.Users.Me, command);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        
        // Create first user
        var (client1, user1) = await _fixture.CreateAuthenticatedClientWithUserAsync(
            email: $"existing_{uniqueId}@example.com");
        
        // Create second user
        var (client2, user2) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        var command = new UpdateCurrentUserCommand
        {
            Username = $"newusername_{uniqueId}",
            Email = $"existing_{uniqueId}@example.com" // Same email as first user
        };

        // Act
        var response = await client2.PutAsJsonAsync(ApiRoutes.Users.Me, command);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Handle_UpdateEmailOnly_UpdatesEmailKeepsUsername()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        
        var command = new UpdateCurrentUserCommand
        {
            Username = user.Username, // Keep same username
            Email = $"newemail_{uniqueId}@example.com" // New email
        };

        // Act
        var response = await client.PutAsJsonAsync(ApiRoutes.Users.Me, command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(user.Username, result.Username); // Username unchanged
        Assert.Equal(command.Email, result.Email); // Email updated
    }
} 