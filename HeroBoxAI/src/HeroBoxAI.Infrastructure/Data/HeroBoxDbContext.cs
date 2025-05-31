using Microsoft.EntityFrameworkCore;
using HeroBoxAI.Domain.Entities;

namespace HeroBoxAI.Infrastructure.Data
{
    public class HeroBoxDbContext : DbContext
    {
        public HeroBoxDbContext(DbContextOptions<HeroBoxDbContext> options) : base(options)
        {
        }

        public DbSet<Hero> Heroes { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Clan> Clans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(HeroBoxDbContext).Assembly);
        }
    }
} 