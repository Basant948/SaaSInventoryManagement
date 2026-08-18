using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Configurations
{
    public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
    {
        public void Configure(EntityTypeBuilder<SalesOrder> builder)
        {
            builder.Property(so => so.SoNumber).HasMaxLength(30).IsRequired();
            builder.Property(so => so.Notes).HasMaxLength(500);
            builder.Property(so => so.Status).HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(so => new { so.TenantId, so.SoNumber }).IsUnique();

            builder.HasOne(so => so.Customer).WithMany().HasForeignKey(so => so.CustomerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(so => so.Warehouse).WithMany().HasForeignKey(so => so.WarehouseId).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(so => so.Lines)
                .WithOne(l => l.SalesOrder)
                .HasForeignKey(l => l.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
