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
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CreateClanCommandTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesClanAndReturnsDto()
    {
        // Arrange
        // First create a user through EF Core since we don't have a user API endpoint yet
        var userId = Guid.NewGuid();
        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HeroBoxAI.Infrastructure.Data.HeroBoxDbContext>();
        
        var user = new User
        {
            Id = userId,
            Username = "TestFounder",
            Email = "founder@example.com",
            PasswordHash = "SomeTestPasswordHash",
            CreatedAt = DateTime.UtcNow
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        var command = new CreateClanCommand
        {
            Name = "New Test Clan",
            Tag = "NEW",
            Description = "New Test Clan Description",
            FounderId = userId
        };
        
        // Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Clans.Base, command);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ClanDto>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Tag, result.Tag);
        Assert.Equal(command.Description, result.Description);
        Assert.Equal(userId, result.FounderId);
        Assert.Equal(user.Username, result.FounderName);
        Assert.Equal(1, result.Level); // Default level is 1
        Assert.Equal(1, result.MemberCount); // Initially just the founder
        
        // Verify clan was persisted to database
        var persistedClan = await dbContext.Clans.FindAsync(result.Id);
        Assert.NotNull(persistedClan);
        Assert.Equal(command.Name, persistedClan.Name);
        Assert.Equal(command.Tag, persistedClan.Tag);
        Assert.Equal(command.Description, persistedClan.Description);
    }
    
    [Fact]
    public async Task Handle_NonExistentFounder_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateClanCommand
        {
            Name = "Invalid Clan",
            Tag = "INV",
            Description = "This clan should not be created",
            FounderId = Guid.NewGuid() // Non-existent user ID
        };
        
        // Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Clans.Base, command);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}