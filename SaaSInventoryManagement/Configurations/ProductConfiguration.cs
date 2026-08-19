using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
            builder.Property(p => p.SKU).HasMaxLength(50).IsRequired();
            builder.Property(p => p.Unit).HasMaxLength(20).IsRequired();
            builder.Property(p => p.Description).HasMaxLength(1000);
            builder.Property(p => p.Image).HasMaxLength(300);

            builder.Property(p => p.CostPrice).HasColumnType("decimal(18,2)");
            builder.Property(p => p.SellingPrice).HasColumnType("decimal(18,2)");

            builder.HasIndex(p => new { p.TenantId, p.SKU });

            builder.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
