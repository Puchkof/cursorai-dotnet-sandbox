using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HeroBoxAI.Application.Users;
using HeroBoxAI.Domain.Enums;
using HeroBoxAI.WebApi.Endpoints;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HeroBoxAI.Application.IntegrationTests.Users.Queries;

public class GetCurrentUserQueryTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GetCurrentUserQueryTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ValidToken_ReturnsCurrentUser()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();

        // Act
        var response = await client.GetAsync(ApiRoutes.Users.Me);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Username, result.Username);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Role, result.Role);
        Assert.Equal(user.Status, result.Status);
    }

    [Fact]
    public async Task Handle_NoAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync(ApiRoutes.Users.Me);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Handle_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await client.GetAsync(ApiRoutes.Users.Me);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _fixture.CreateClient();
        // This is a token that's clearly expired (issued in the past)
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", expiredToken);

        // Act
        var response = await client.GetAsync(ApiRoutes.Users.Me);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Handle_MalformedToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "malformed.token");

        // Act
        var response = await client.GetAsync(ApiRoutes.Users.Me);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Handle_UserWithClan_ReturnsUserWithClanInfo()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        // Create a clan and assign user to it
        using (var scope = _fixture.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HeroBoxAI.Infrastructure.Data.HeroBoxDbContext>();
            
            var clan = new HeroBoxAI.Domain.Entities.Clan
            {
                Id = Guid.NewGuid(),
                Name = "Test Clan",
                Tag = "TEST",
                Description = "Test clan description",
                FounderId = user.Id,
                Level = 1,
                CreatedAt = DateTime.UtcNow
            };
            
            dbContext.Clans.Add(clan);
            
            // Update user's clan
            user.ClanId = clan.Id;
            dbContext.Users.Update(user);
            
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync(ApiRoutes.Users.Me);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.NotNull(result.ClanId);
        Assert.Equal("Test Clan", result.ClanName);
    }

    [Fact]
    public async Task Handle_UserWithoutClan_ReturnsUserWithNullClanInfo()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();

        // Act
        var response = await client.GetAsync(ApiRoutes.Users.Me);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Null(result.ClanId);
        Assert.Null(result.ClanName);
        Assert.Equal(0, result.HeroCount);
    }
} 