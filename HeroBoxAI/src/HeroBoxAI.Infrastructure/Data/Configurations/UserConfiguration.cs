using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HeroBoxAI.Domain.Entities;

namespace HeroBoxAI.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Primary key
            builder.HasKey(u => u.Id);
            
            // Properties
            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(u => u.PasswordHash)
                .IsRequired();
                
            builder.Property(u => u.Role)
                .IsRequired()
                .HasDefaultValue(HeroBoxAI.Domain.Enums.UserRole.Player);
                
            builder.Property(u => u.Status)
                .IsRequired()
                .HasDefaultValue(HeroBoxAI.Domain.Enums.UserStatus.Active);
                
            builder.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            // Indexes
            builder.HasIndex(u => u.Username).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();
                
            // Relationships
            
            // User has many Heroes
            builder.HasMany(u => u.Heroes)
                .WithOne(h => h.User)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // User belongs to one Clan (optional)
            builder.HasOne(u => u.Clan)
                .WithMany(c => c.Members)
                .HasForeignKey(u => u.ClanId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
} 