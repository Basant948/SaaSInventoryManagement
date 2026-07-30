using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.Property(a => a.UserId).HasMaxLength(450); 
            builder.Property(a => a.Username).HasMaxLength(256);
            builder.Property(a => a.EntityName).HasMaxLength(200);
            builder.Property(a => a.EntityId).HasMaxLength(200);
            builder.Property(a => a.CorrelationId).HasMaxLength(100);
            builder.Property(a => a.IPAddress).HasMaxLength(45);
            builder.Property(a => a.RequestPath).HasMaxLength(300);

            builder.HasIndex(a => new { a.TenantId, a.CreatedAt });
            builder.HasIndex(a => a.EntityName);
            builder.HasIndex(a => a.Action);
            builder.HasIndex(a => a.UserId);
        }
    }
}