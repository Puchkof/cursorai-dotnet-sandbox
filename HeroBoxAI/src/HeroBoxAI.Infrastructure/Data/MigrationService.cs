using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HeroBoxAI.Infrastructure.Data
{
    public static class MigrationService
    {
        public static async Task MigrateDatabaseAsync(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<HeroBoxDbContext>>();

            try
            {
                var context = services.GetRequiredService<HeroBoxDbContext>();
                
                logger.LogInformation("Starting database migration");
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migration completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database");
                throw;
            }
        }
    }
} 