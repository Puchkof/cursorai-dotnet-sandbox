using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HeroBoxAI.Application.Clans;
using HeroBoxAI.Application.Clans.Commands.CreateClan;
using HeroBoxAI.Domain.Entities;
using HeroBoxAI.WebApi.Endpoints;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HeroBoxAI.Application.IntegrationTests.Clans.Commands;

public class CreateClanCommandTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CreateClanCommandTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesClanAndReturnsDto()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var command = new CreateClanCommand
        {
            Name = $"New Test Clan {uniqueId}",
            Tag = $"NT{uniqueId[..2]}",
            Description = "New Test Clan Description"
        };
        
        // Act
        var response = await client.PostAsJsonAsync(ApiRoutes.Clans.Base, command);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ClanDto>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Tag, result.Tag);
        Assert.Equal(command.Description, result.Description);
        Assert.Equal(user.Id, result.FounderId);
        Assert.Equal(user.Username, result.FounderName);
        Assert.Equal(1, result.Level); // Default level is 1
        Assert.Equal(1, result.MemberCount); // Initially just the founder
        
        // Verify clan was persisted to database
        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HeroBoxAI.Infrastructure.Data.HeroBoxDbContext>();
        var persistedClan = await dbContext.Clans.FindAsync(result.Id);
        Assert.NotNull(persistedClan);
        Assert.Equal(command.Name, persistedClan.Name);
        Assert.Equal(command.Tag, persistedClan.Tag);
        Assert.Equal(command.Description, persistedClan.Description);
        Assert.Equal(user.Id, persistedClan.FounderId);
    }
    
    [Fact]
    public async Task Handle_NoAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var command = new CreateClanCommand
        {
            Name = "Test Clan",
            Tag = "TEST",
            Description = "Test Description"
        };
        
        // Act
        var response = await client.PostAsJsonAsync(ApiRoutes.Clans.Base, command);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Handle_InvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");
        
        var command = new CreateClanCommand
        {
            Name = "Test Clan",
            Tag = "TEST",
            Description = "Test Description"
        };
        
        // Act
        var response = await client.PostAsJsonAsync(ApiRoutes.Clans.Base, command);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Handle_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        var command = new CreateClanCommand
        {
            Name = "", // Empty name
            Tag = "TEST",
            Description = "Test Description"
        };
        
        // Act
        var response = await client.PostAsJsonAsync(ApiRoutes.Clans.Base, command);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Handle_EmptyTag_ReturnsBadRequest()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        var command = new CreateClanCommand
        {
            Name = "Test Clan",
            Tag = "", // Empty tag
            Description = "Test Description"
        };
        
        // Act
        var response = await client.PostAsJsonAsync(ApiRoutes.Clans.Base, command);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Handle_UserAlreadyInClan_ReturnsConflict()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        // Create first clan with unique names
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var firstCommand = new CreateClanCommand
        {
            Name = $"First Clan {uniqueId}",
            Tag = $"F1{uniqueId[..2]}",
            Description = "First clan description"
        };
        
        await client.PostAsJsonAsync(ApiRoutes.Clans.Base, firstCommand);
        
        // Try to create second clan with same user
        var secondCommand = new CreateClanCommand
        {
            Name = $"Second Clan {uniqueId}",
            Tag = $"S2{uniqueId[..2]}",
            Description = "Second clan description"
        };
        
        // Act
        var response = await client.PostAsJsonAsync(ApiRoutes.Clans.Base, secondCommand);
        
        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}