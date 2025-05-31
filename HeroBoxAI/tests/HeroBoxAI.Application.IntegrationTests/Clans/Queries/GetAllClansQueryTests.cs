using System;
using System.Collections.Generic;
using System.Linq;
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

public class GetAllClansQueryTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GetAllClansQueryTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Handle_ReturnsAllClans()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HeroBoxAI.Infrastructure.Data.HeroBoxDbContext>();
        
        // Clear existing clans to ensure clean state
        dbContext.Clans.RemoveRange(dbContext.Clans);
        await dbContext.SaveChangesAsync();
        
        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Username = "TestUser1",
            Email = "test1@example.com",
            PasswordHash = "SomeTestPasswordHash1",
            CreatedAt = DateTime.UtcNow
        };
        
        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Username = "TestUser2",
            Email = "test2@example.com",
            PasswordHash = "SomeTestPasswordHash2",
            CreatedAt = DateTime.UtcNow
        };
        
        var clan1 = new Clan
        {
            Id = Guid.NewGuid(),
            Name = "Test Clan 1",
            Tag = "TST1",
            Description = "Test Clan 1 Description",
            FounderId = user1.Id,
            Founder = user1,
            Level = 1,
            CreatedAt = DateTime.UtcNow
        };
        
        var clan2 = new Clan
        {
            Id = Guid.NewGuid(),
            Name = "Test Clan 2",
            Tag = "TST2",
            Description = "Test Clan 2 Description",
            FounderId = user2.Id,
            Founder = user2,
            Level = 2,
            CreatedAt = DateTime.UtcNow
        };
        
        dbContext.Users.AddRange(user1, user2);
        dbContext.Clans.AddRange(clan1, clan2);
        await dbContext.SaveChangesAsync();
        
        // Act
        var response = await _client.GetAsync(ApiRoutes.Clans.Base);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<List<ClanDto>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var clanDto1 = result.FirstOrDefault(c => c.Id == clan1.Id);
        Assert.NotNull(clanDto1);
        Assert.Equal(clan1.Name, clanDto1.Name);
        Assert.Equal(clan1.Tag, clanDto1.Tag);
        Assert.Equal(user1.Username, clanDto1.FounderName);
        
        var clanDto2 = result.FirstOrDefault(c => c.Id == clan2.Id);
        Assert.NotNull(clanDto2);
        Assert.Equal(clan2.Name, clanDto2.Name);
        Assert.Equal(clan2.Tag, clanDto2.Tag);
        Assert.Equal(user2.Username, clanDto2.FounderName);
    }
    
    [Fact]
    public async Task Handle_NoClans_ReturnsEmptyList()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HeroBoxAI.Infrastructure.Data.HeroBoxDbContext>();
        
        // Clear existing clans to ensure clean state
        dbContext.Clans.RemoveRange(dbContext.Clans);
        await dbContext.SaveChangesAsync();
        
        // Act
        var response = await _client.GetAsync(ApiRoutes.Clans.Base);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<List<ClanDto>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}