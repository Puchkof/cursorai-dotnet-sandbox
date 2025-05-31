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
using Microsoft.EntityFrameworkCore;

namespace HeroBoxAI.Application.IntegrationTests.Clans.Commands;

public class DeleteClanCommandTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public DeleteClanCommandTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ExistingClan_DeletesClanAndReturnsNoContent()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        // Create a clan first
        var createCommand = new CreateClanCommand
        {
            Name = "Clan To Delete",
            Tag = "DEL",
            Description = "This clan will be deleted"
        };
        
        var createResponse = await client.PostAsJsonAsync(ApiRoutes.Clans.Base, createCommand);
        var createdClan = await createResponse.Content.ReadFromJsonAsync<ClanDto>(_jsonOptions);
        
        // Verify clan exists before deletion
        using (var scope = _fixture.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HeroBoxAI.Infrastructure.Data.HeroBoxDbContext>();
            var existingClan = await dbContext.Clans.FindAsync(createdClan!.Id);
            Assert.NotNull(existingClan);
        }
        
        // Act
        var response = await client.DeleteAsync($"{ApiRoutes.Clans.Base}/{createdClan!.Id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify clan was deleted from database
        using (var scope = _fixture.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HeroBoxAI.Infrastructure.Data.HeroBoxDbContext>();
            var deletedClan = await dbContext.Clans.AsNoTracking().FirstOrDefaultAsync(c => c.Id == createdClan.Id);
            Assert.Null(deletedClan);
        }
    }

    [Fact]
    public async Task Handle_NoAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var clanId = Guid.NewGuid();
        
        // Act
        var response = await client.DeleteAsync($"{ApiRoutes.Clans.Base}/{clanId}");
        
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
        
        // Act
        var response = await client.DeleteAsync($"{ApiRoutes.Clans.Base}/{clanId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task Handle_NonExistentClan_ReturnsNotFound()
    {
        // Arrange
        var (client, user) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        var nonExistentClanId = Guid.NewGuid();
        
        // Act
        var response = await client.DeleteAsync($"{ApiRoutes.Clans.Base}/{nonExistentClanId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Handle_UserNotClanFounder_ReturnsForbidden()
    {
        // Arrange
        var (client1, user1) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        var (client2, user2) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        // User1 creates a clan
        var createCommand = new CreateClanCommand
        {
            Name = "User1 Clan",
            Tag = "U1",
            Description = "User1's clan"
        };
        
        var createResponse = await client1.PostAsJsonAsync(ApiRoutes.Clans.Base, createCommand);
        var createdClan = await createResponse.Content.ReadFromJsonAsync<ClanDto>(_jsonOptions);
        
        // User2 tries to delete User1's clan
        // Act
        var response = await client2.DeleteAsync($"{ApiRoutes.Clans.Base}/{createdClan!.Id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task Handle_ClanWithMembers_DeletesClanAndUpdatesMembers()
    {
        // Arrange
        var (client, founder) = await _fixture.CreateAuthenticatedClientWithUserAsync();
        
        // Create a clan
        var createCommand = new CreateClanCommand
        {
            Name = "Clan With Members",
            Tag = "MEM",
            Description = "This clan has members"
        };
        
        var createResponse = await client.PostAsJsonAsync(ApiRoutes.Clans.Base, createCommand);
        var createdClan = await createResponse.Content.ReadFromJsonAsync<ClanDto>(_jsonOptions);
        
        // Add a member to the clan manually through database
        using (var scope = _fixture.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HeroBoxAI.Infrastructure.Data.HeroBoxDbContext>();
            
            // Create member without ClanId first to avoid foreign key constraint violation
            var member = new User
            {
                Id = Guid.NewGuid(),
                Username = "ClanMember",
                Email = "member@example.com",
                PasswordHash = "SomeTestPasswordHash",
                CreatedAt = DateTime.UtcNow,
                ClanId = null // Set to null initially
            };
            
            dbContext.Users.Add(member);
            await dbContext.SaveChangesAsync();
            
            // Now update the member's ClanId
            member.ClanId = createdClan!.Id;
            dbContext.Users.Update(member);
            await dbContext.SaveChangesAsync();
            
            // Verify setup
            var existingClan = await dbContext.Clans.FindAsync(createdClan.Id);
            var existingMember = await dbContext.Users.FindAsync(member.Id);
            Assert.NotNull(existingClan);
            Assert.NotNull(existingMember);
            Assert.Equal(createdClan.Id, existingMember.ClanId);
        }
        
        // Act
        var response = await client.DeleteAsync($"{ApiRoutes.Clans.Base}/{createdClan!.Id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify clan was deleted and member's clan reference was set to null
        using (var scope = _fixture.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HeroBoxAI.Infrastructure.Data.HeroBoxDbContext>();
            
            var deletedClan = await dbContext.Clans.AsNoTracking().FirstOrDefaultAsync(c => c.Id == createdClan.Id);
            Assert.Null(deletedClan);
            
            var updatedMember = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == "ClanMember");
            Assert.NotNull(updatedMember);
            Assert.Null(updatedMember.ClanId);
        }
    }
} 