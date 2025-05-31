using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HeroBoxAI.Domain.Repositories;
using HeroBoxAI.Infrastructure.Data;
using HeroBoxAI.Infrastructure.Repositories;
using HeroBoxAI.Application.Common.Interfaces;
using HeroBoxAI.Infrastructure.Services;
using System.Threading.Tasks;

namespace HeroBoxAI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext with PostgreSQL
            services.AddDbContext<HeroBoxDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(HeroBoxDbContext).Assembly.FullName)));
            
            // Register repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IClanRepository, ClanRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            
            // Register authentication services
            services.AddScoped<IPasswordHashService, PasswordHashService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IUserContext, UserContext>();
            
            // Register HttpContextAccessor for UserContext
            services.AddHttpContextAccessor();
            
            return services;
        }
        
        public static async Task ApplyMigrationsAsync(this IHost host)
        {
            await MigrationService.MigrateDatabaseAsync(host);
        }
    }
} 