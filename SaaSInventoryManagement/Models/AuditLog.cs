using SaaSInventoryManagement.Enums;
using SaaSInventoryManagement.Models.Base;

namespace SaaSInventoryManagement.Models
{
    public class AuditLog : ITenantOwned
    {
        public int Id { get; set; }
        public int TenantId { get; set; }

        public string UserId { get; set; }
        public string Username { get; set; }

        public AuditAction Action { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }

        public string? OldValues { get; set; } = null;
        public string? NewValues { get; set; }

        public string CorrelationId { get; set; }
        public string IPAddress { get; set; }
        public string RequestPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
