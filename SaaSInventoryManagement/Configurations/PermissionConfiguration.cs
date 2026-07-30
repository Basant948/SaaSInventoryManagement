using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.Property(p => p.Key).HasMaxLength(150);
            builder.Property(p => p.DisplayName).HasMaxLength(150);
            builder.Property(p => p.GroupName).HasMaxLength(100);
            builder.Property(p => p.IconClass).HasMaxLength(100);
            builder.Property(p => p.ControllerAction).HasMaxLength(150);

            builder.HasIndex(p => p.Key).IsUnique();
        }
    }
}
