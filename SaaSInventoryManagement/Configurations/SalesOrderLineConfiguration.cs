using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Configurations
{
    public class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
    {
        public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
        {
            builder.Property(l => l.QuantityOrdered).HasColumnType("decimal(18,3)");
            builder.Property(l => l.QuantityFulfilled).HasColumnType("decimal(18,3)");
            builder.Property(l => l.UnitPrice).HasColumnType("decimal(18,2)");

            builder.HasOne(l => l.Product).WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
