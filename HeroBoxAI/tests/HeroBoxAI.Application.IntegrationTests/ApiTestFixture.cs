using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using HeroBoxAI.Application.Common.Interfaces;
using HeroBoxAI.Domain.Entities;
using HeroBoxAI.Domain.Enums;
using HeroBoxAI.Infrastructure;
using HeroBoxAI.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;

namespace HeroBoxAI.Application.IntegrationTests;

public class ApiTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private string _connectionString = string.Empty;

    public ApiTestFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("herobox_test")
            .WithUsername("postgres_test")
            .WithPassword("postgres_test")
            .WithCleanUp(true)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddInMemoryCollection(
                new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = _connectionString
                });
        });

        builder.ConfigureServices(services =>
        {
            // Modify the DbContext to create the schema on startup
            services.AddScoped<HeroBoxDbContextInitializer>();
        });
    }

    public new HttpClient CreateClient()
    {
        return base.CreateClient();
    }

    /// <summary>
    /// Creates an authenticated HTTP client with a JWT token for the specified user
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync(User user)
    {
        var client = CreateClient();
        
        using var scope = Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var token = jwtTokenService.GenerateToken(user);
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Creates a test user and returns an authenticated HTTP client
    /// </summary>
    public async Task<(HttpClient Client, User User)> CreateAuthenticatedClientWithUserAsync(
        string? username = null, 
        string? email = null, 
        UserRole role = UserRole.Player)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username ?? $"testuser_{uniqueId}",
            Email = email ?? $"test_{uniqueId}@example.com",
            PasswordHash = "hashedpassword", // This won't be used for JWT generation
            Role = role,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        // Add user to database
        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HeroBoxDbContext>();
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        var client = await CreateAuthenticatedClientAsync(user);
        return (client, user);
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        _connectionString = _postgresContainer.GetConnectionString();

        // Apply migrations manually since we don't want to run the web app migrations
        using (var scope = Services.CreateScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<HeroBoxDbContextInitializer>();
            await initializer.InitializeAsync();
        }
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}

// Helper class to initialize and migrate the database
public class HeroBoxDbContextInitializer
{
    private readonly HeroBoxDbContext _context;
    private readonly ILogger<HeroBoxDbContextInitializer> _logger;

    public HeroBoxDbContextInitializer(
        HeroBoxDbContext context,
        ILogger<HeroBoxDbContextInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Ensuring database exists and applying migrations");
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the test database");
            throw;
        }
    }
} 