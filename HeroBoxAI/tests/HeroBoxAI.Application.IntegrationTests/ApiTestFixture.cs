using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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