using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Configurations
{
    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.Property(w => w.Name).HasMaxLength(150).IsRequired();
            builder.Property(w => w.Code).HasMaxLength(20).IsRequired();
            builder.Property(w => w.Address).HasMaxLength(300);

            builder.HasIndex(w => new { w.TenantId, w.Code });
        }
    }
}
