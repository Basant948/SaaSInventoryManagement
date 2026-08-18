using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Configurations
{
    public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
        {
            builder.Property(po => po.PoNumber).HasMaxLength(30).IsRequired();
            builder.Property(po => po.Notes).HasMaxLength(500);
            builder.Property(po => po.Status).HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(po => new { po.TenantId, po.PoNumber }).IsUnique();

            builder.HasOne(po => po.Supplier).WithMany().HasForeignKey(po => po.SupplierId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(po => po.Warehouse).WithMany().HasForeignKey(po => po.WarehouseId).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(po => po.Lines)
                .WithOne(l => l.PurchaseOrder)
                .HasForeignKey(l => l.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
