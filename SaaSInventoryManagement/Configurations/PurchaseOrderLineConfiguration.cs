using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Configurations
{
    public class PurchaseOrderLineConfiguration : IEntityTypeConfiguration<PurchaseOrderLine>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
        {
            builder.Property(l => l.QuantityOrdered).HasColumnType("decimal(18,3)");
            builder.Property(l => l.QuantityReceived).HasColumnType("decimal(18,3)");
            builder.Property(l => l.UnitCost).HasColumnType("decimal(18,2)");

            builder.HasOne(l => l.Product).WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
