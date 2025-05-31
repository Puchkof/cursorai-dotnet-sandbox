using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HeroBoxAI.Domain.Entities;

namespace HeroBoxAI.Infrastructure.Data.Configurations
{
    public class HeroConfiguration : IEntityTypeConfiguration<Hero>
    {
        public void Configure(EntityTypeBuilder<Hero> builder)
        {
            // Primary key
            builder.HasKey(h => h.Id);
            
            // Properties
            builder.Property(h => h.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(h => h.Class)
                .IsRequired();
                
            builder.Property(h => h.Level)
                .IsRequired()
                .HasDefaultValue(1);
                
            builder.Property(h => h.Experience)
                .IsRequired()
                .HasDefaultValue(0);
                
            builder.Property(h => h.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            // Relationships
            
            // Hero belongs to one User
            builder.HasOne(h => h.User)
                .WithMany(u => u.Heroes)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Hero has many Items
            builder.HasMany(h => h.Items)
                .WithOne(i => i.Hero)
                .HasForeignKey(i => i.HeroId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 