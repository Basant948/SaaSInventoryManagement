using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Configurations
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.Property(s => s.Name).HasMaxLength(150).IsRequired();
            builder.Property(s => s.ContactPerson).HasMaxLength(150);
            builder.Property(s => s.Email).HasMaxLength(150);
            builder.Property(s => s.Phone).HasMaxLength(30);
            builder.Property(s => s.Address).HasMaxLength(300);

            builder.HasIndex(s => new { s.TenantId, s.Name });
        }
    }
}
