using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HeroBoxAI.Domain.Entities;

namespace HeroBoxAI.Infrastructure.Data.Configurations
{
    public class ClanConfiguration : IEntityTypeConfiguration<Clan>
    {
        public void Configure(EntityTypeBuilder<Clan> builder)
        {
            // Primary key
            builder.HasKey(c => c.Id);
            
            // Properties
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(c => c.Tag)
                .IsRequired()
                .HasMaxLength(10);
                
            builder.Property(c => c.Description)
                .HasMaxLength(500);
                
            builder.Property(c => c.Level)
                .IsRequired()
                .HasDefaultValue(1);
                
            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            // Indexes
            builder.HasIndex(c => c.Name).IsUnique();
            builder.HasIndex(c => c.Tag).IsUnique();
                
            // Relationships
            
            // Clan has one Founder (User)
            builder.HasOne(c => c.Founder)
                .WithMany()
                .HasForeignKey(c => c.FounderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Clan has many Members (Users)
            builder.HasMany(c => c.Members)
                .WithOne(u => u.Clan)
                .HasForeignKey(u => u.ClanId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
} 