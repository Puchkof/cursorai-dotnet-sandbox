using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HeroBoxAI.Application.Clans;
using HeroBoxAI.Domain.Entities;
using HeroBoxAI.WebApi.Endpoints;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HeroBoxAI.Application.IntegrationTests.Clans.Queries;

public class GetClanByIdQueryTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GetClanByIdQueryTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Handle_ExistingClanId_ReturnsClanDto()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HeroBoxAI.Infrastructure.Data.HeroBoxDbContext>();
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "TestUser",
            Email = "test@example.com",
            PasswordHash = "SomeTestPasswordHash",
            CreatedAt = DateTime.UtcNow
        };
        
        var clan = new Clan
        {
            Id = Guid.NewGuid(),
            Name = "Test Clan",
            Tag = "TEST",
            Description = "Test Clan Description",
            FounderId = user.Id,
            Founder = user,
            Level = 1,
            CreatedAt = DateTime.UtcNow
        };
        
        dbContext.Users.Add(user);
        dbContext.Clans.Add(clan);
        await dbContext.SaveChangesAsync();
        
        // Act
        var response = await _client.GetAsync($"{ApiRoutes.Clans.Base}/{clan.Id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ClanDto>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(clan.Id, result.Id);
        Assert.Equal(clan.Name, result.Name);
        Assert.Equal(clan.Tag, result.Tag);
        Assert.Equal(clan.Description, result.Description);
        Assert.Equal(clan.Level, result.Level);
        Assert.Equal(clan.FounderId, result.FounderId);
        Assert.Equal(user.Username, result.FounderName);
        Assert.Equal(0, result.MemberCount); // Initially no members
    }
    
    [Fact]
    public async Task Handle_NonExistentClanId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentClanId = Guid.NewGuid(); // Non-existent ID
        
        // Act
        var response = await _client.GetAsync($"{ApiRoutes.Clans.Base}/{nonExistentClanId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
} 