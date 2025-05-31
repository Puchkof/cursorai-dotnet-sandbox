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

public class GetClanMembersQueryTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly ApiTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GetClanMembersQueryTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Handle_ExistingClanWithMembers_ReturnsClanMembers()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HeroBoxAI.Infrastructure.Data.HeroBoxDbContext>();
        
        // Clear existing data to ensure clean state
        dbContext.Clans.RemoveRange(dbContext.Clans);
        dbContext.Users.RemoveRange(dbContext.Users);
        await dbContext.SaveChangesAsync();
        
        var founder = new User
        {
            Id = Guid.NewGuid(),
            Username = "ClanFounder",
            Email = "founder@example.com",
            PasswordHash = "SomeTestPasswordHash",
            CreatedAt = DateTime.UtcNow
        };
        
        var member1 = new User
        {
            Id = Guid.NewGuid(),
            Username = "Member1",
            Email = "member1@example.com",
            PasswordHash = "SomeTestPasswordHash",
            CreatedAt = DateTime.UtcNow
        };
        
        var member2 = new User
        {
            Id = Guid.NewGuid(),
            Username = "Member2",
            Email = "member2@example.com",
            PasswordHash = "SomeTestPasswordHash",
            CreatedAt = DateTime.UtcNow
        };
        
        var clan = new Clan
        {
            Id = Guid.NewGuid(),
            Name = "Test Clan",
            Tag = "TEST",
            Description = "Test Clan Description",
            FounderId = founder.Id,
            Level = 1,
            CreatedAt = DateTime.UtcNow
        };
        
        // Set clan membership
        member1.ClanId = clan.Id;
        member2.ClanId = clan.Id;
        
        dbContext.Users.AddRange(founder, member1, member2);
        dbContext.Clans.Add(clan);
        await dbContext.SaveChangesAsync();
        
        // Act
        var response = await _client.GetAsync($"{ApiRoutes.Clans.Base}/{clan.Id}/members");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<List<ClanMemberDto>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Only members, not founder
        
        var memberDto1 = result.FirstOrDefault(m => m.Id == member1.Id);
        Assert.NotNull(memberDto1);
        Assert.Equal(member1.Username, memberDto1.Username);
        Assert.Equal(member1.Email, memberDto1.Email);
        
        var memberDto2 = result.FirstOrDefault(m => m.Id == member2.Id);
        Assert.NotNull(memberDto2);
        Assert.Equal(member2.Username, memberDto2.Username);
        Assert.Equal(member2.Email, memberDto2.Email);
    }
    
    [Fact]
    public async Task Handle_ExistingClanWithNoMembers_ReturnsEmptyList()
    {
        // Arrange
        using var scope = _fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HeroBoxAI.Infrastructure.Data.HeroBoxDbContext>();
        
        // Clear existing data to ensure clean state
        dbContext.Clans.RemoveRange(dbContext.Clans);
        dbContext.Users.RemoveRange(dbContext.Users);
        await dbContext.SaveChangesAsync();
        
        var founder = new User
        {
            Id = Guid.NewGuid(),
            Username = "ClanFounder",
            Email = "founder@example.com",
            PasswordHash = "SomeTestPasswordHash",
            CreatedAt = DateTime.UtcNow
        };
        
        var clan = new Clan
        {
            Id = Guid.NewGuid(),
            Name = "Empty Clan",
            Tag = "EMPTY",
            Description = "Clan with no members",
            FounderId = founder.Id,
            Level = 1,
            CreatedAt = DateTime.UtcNow
        };
        
        dbContext.Users.Add(founder);
        dbContext.Clans.Add(clan);
        await dbContext.SaveChangesAsync();
        
        // Act
        var response = await _client.GetAsync($"{ApiRoutes.Clans.Base}/{clan.Id}/members");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<List<ClanMemberDto>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_NonExistentClan_ReturnsEmptyList()
    {
        // Arrange
        var nonExistentClanId = Guid.NewGuid();
        
        // Act
        var response = await _client.GetAsync($"{ApiRoutes.Clans.Base}/{nonExistentClanId}/members");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<List<ClanMemberDto>>(_jsonOptions);
        Assert.NotNull(result);
        Assert.Empty(result);
    }
} 