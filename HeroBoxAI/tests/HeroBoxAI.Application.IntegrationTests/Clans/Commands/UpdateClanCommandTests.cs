using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HeroBoxAI.Application.Clans;
using HeroBoxAI.Application.Clans.Commands.CreateClan;
using HeroBoxAI.Application.Clans.Commands.UpdateClan;
using HeroBoxAI.Domain.Entities;
using HeroBoxAI.WebApi.Endpoints;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HeroBoxAI.Application.IntegrationTests.Clans.Commands;

public class UpdateClanCommandTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public UpdateClanCommandTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesClanAndReturnsDto()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        // Create a clan first with unique name
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var createCommand = new CreateClanCommand
        {
            Name = $"Original Clan {uniqueId}",
            Tag = $"OR{uniqueId[..2]}",
            Description = "Original Description"
        };
        
        var createResponse = await client.PostAsJsonAsync(ApiRoutes.Clans.Base, createCommand);
        var createdClan = await createResponse.Content.ReadFromJsonAsync<ClanDto>(_jsonOptions);
        
        var updateCommand = new UpdateClanCommand
        {
            Id = createdClan!.Id,
            Name = $"Updated Clan {uniqueId}",
            Tag = $"UP{uniqueId[..2]}",
            Description = "Updated Description"
        };
        
        // Act
        var response = await client.PutAsJsonAsync($"{ApiRoutes.Clans.Base}/{createdClan.Id}", updateCommand);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ClanDto>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(updateCommand.Name, result.Name);
        Assert.Equal(updateCommand.Tag, result.Tag);
        Assert.Equal(updateCommand.Description, result.Description);
        Assert.Equal(createdClan.Id, result.Id);
        Assert.Equal(user.Id, result.FounderId);
        Assert.Equal(user.Username, result.FounderName);
        Assert.Equal(1, result.Level); // Level should remain unchanged
        
        // Verify clan was updated in database
        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HeroBoxAI.Infrastructure.Data.HeroBoxDbContext>();
        var persistedClan = await dbContext.Clans.FindAsync(createdClan.Id);
        Assert.NotNull(persistedClan);
        Assert.Equal(updateCommand.Name, persistedClan.Name);
        Assert.Equal(updateCommand.Tag, persistedClan.Tag);
        Assert.Equal(updateCommand.Description, persistedClan.Description);
    }
    
    [Fact]
    public async Task Handle_NoAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var clanId = Guid.NewGuid();
        var updateCommand = new UpdateClanCommand
        {
            Id = clanId,
            Name = "Updated Name",
            Tag = "UPD",
            Description = "Updated Description"
        };
        
        // Act
        var response = await client.PutAsJsonAsync($"{ApiRoutes.Clans.Base}/{clanId}", updateCommand);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Handle_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");
        
        var clanId = Guid.NewGuid();
        var updateCommand = new UpdateClanCommand
        {
            Id = clanId,
            Name = "Updated Name",
            Tag = "UPD",
            Description = "Updated Description"
        };
        
        // Act
        var response = await client.PutAsJsonAsync($"{ApiRoutes.Clans.Base}/{clanId}", updateCommand);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task Handle_NonExistentClan_ReturnsNotFound()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        var nonExistentClanId = Guid.NewGuid();
        var updateCommand = new UpdateClanCommand
        {
            Id = nonExistentClanId,
            Name = "Updated Name",
            Tag = "UPD",
            Description = "Updated Description"
        };
        
        // Act
        var response = await client.PutAsJsonAsync($"{ApiRoutes.Clans.Base}/{nonExistentClanId}", updateCommand);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Handle_UserNotClanFounder_ReturnsForbidden()
    {
        // Arrange
        var (client1, user1) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        var (client2, user2) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        // User1 creates a clan with unique name
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var createCommand = new CreateClanCommand
        {
            Name = $"User1 Clan {uniqueId}",
            Tag = $"U1{uniqueId[..2]}",
            Description = "User1's clan"
        };
        
        var createResponse = await client1.PostAsJsonAsync(ApiRoutes.Clans.Base, createCommand);
        var createdClan = await createResponse.Content.ReadFromJsonAsync<ClanDto>(_jsonOptions);
        
        // User2 tries to update User1's clan
        var updateCommand = new UpdateClanCommand
        {
            Id = createdClan!.Id,
            Name = $"Hacked Clan {uniqueId}",
            Tag = $"HK{uniqueId[..2]}",
            Description = "This should not work"
        };
        
        // Act
        var response = await client2.PutAsJsonAsync($"{ApiRoutes.Clans.Base}/{createdClan.Id}", updateCommand);
        
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task Handle_MismatchedIds_UpdatesWithRouteId()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        // Create a clan first with unique name
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var createCommand = new CreateClanCommand
        {
            Name = $"Original Clan {uniqueId}",
            Tag = $"OR{uniqueId[..2]}",
            Description = "Original Description"
        };
        
        var createResponse = await client.PostAsJsonAsync(ApiRoutes.Clans.Base, createCommand);
        var createdClan = await createResponse.Content.ReadFromJsonAsync<ClanDto>(_jsonOptions);
        
        var updateCommand = new UpdateClanCommand
        {
            Id = Guid.NewGuid(), // Different ID than route
            Name = $"Updated Clan {uniqueId}",
            Tag = $"UP{uniqueId[..2]}",
            Description = "Updated Description"
        };
        
        // Act - Route ID should take precedence
        var response = await client.PutAsJsonAsync($"{ApiRoutes.Clans.Base}/{createdClan!.Id}", updateCommand);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ClanDto>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(createdClan.Id, result.Id); // Should use route ID, not command ID
        Assert.Equal(updateCommand.Name, result.Name);
        Assert.Equal(updateCommand.Tag, result.Tag);
        Assert.Equal(updateCommand.Description, result.Description);
    }

    [Fact]
    public async Task Handle_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        // Create a clan first with unique name
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var createCommand = new CreateClanCommand
        {
            Name = $"Original Clan {uniqueId}",
            Tag = $"OR{uniqueId[..2]}",
            Description = "Original Description"
        };
        
        var createResponse = await client.PostAsJsonAsync(ApiRoutes.Clans.Base, createCommand);
        var createdClan = await createResponse.Content.ReadFromJsonAsync<ClanDto>(_jsonOptions);
        
        var updateCommand = new UpdateClanCommand
        {
            Id = createdClan!.Id,
            Name = "", // Empty name
            Tag = $"UP{uniqueId[..2]}",
            Description = "Updated Description"
        };
        
        // Act
        var response = await client.PutAsJsonAsync($"{ApiRoutes.Clans.Base}/{createdClan.Id}", updateCommand);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
} 