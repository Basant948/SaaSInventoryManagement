using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
            builder.Property(c => c.ContactPerson).HasMaxLength(150);
            builder.Property(c => c.Email).HasMaxLength(150);
            builder.Property(c => c.Phone).HasMaxLength(30);
            builder.Property(c => c.Address).HasMaxLength(300);

            builder.HasIndex(c => new { c.TenantId, c.Name });
        }
    }
}
