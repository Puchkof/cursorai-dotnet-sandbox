using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HeroBoxAI.Domain.Entities;

namespace HeroBoxAI.Infrastructure.Data.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            // Primary key
            builder.HasKey(i => i.Id);
            
            // Properties
            builder.Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(i => i.Description)
                .HasMaxLength(500);
                
            builder.Property(i => i.Type)
                .IsRequired();
                
            builder.Property(i => i.Rarity)
                .IsRequired();
                
            builder.Property(i => i.RequiredLevel)
                .IsRequired()
                .HasDefaultValue(1);
                
            builder.Property(i => i.Quantity)
                .IsRequired()
                .HasDefaultValue(1);
                
            builder.Property(i => i.IsEquipped)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(i => i.AcquiredAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            // Relationships
            
            // Item belongs to one Hero
            builder.HasOne(i => i.Hero)
                .WithMany(h => h.Items)
                .HasForeignKey(i => i.HeroId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 