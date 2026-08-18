using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Configurations
{
    public class StockLedgerEntryConfiguration : IEntityTypeConfiguration<StockLedgerEntry>
    {
        public void Configure(EntityTypeBuilder<StockLedgerEntry> builder)
        {
            builder.Property(l => l.QuantityChange).HasColumnType("decimal(18,3)");
            builder.Property(l => l.BalanceAfter).HasColumnType("decimal(18,3)");
            builder.Property(l => l.ReferenceType).HasMaxLength(50);
            builder.Property(l => l.Notes).HasMaxLength(300);
            builder.Property(l => l.CreatedBy).HasMaxLength(256);

            builder.HasIndex(l => new { l.TenantId, l.ProductId, l.WarehouseId, l.CreatedAt });
            builder.HasIndex(l => new { l.ReferenceType, l.ReferenceId });

            builder.HasOne(l => l.Product).WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(l => l.Warehouse).WithMany().HasForeignKey(l => l.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
