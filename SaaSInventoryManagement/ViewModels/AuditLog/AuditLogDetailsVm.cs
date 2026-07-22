using SaaSInventoryManagement.Enums;

namespace SaaSInventoryManagement.ViewModels.AuditLog
{
    public class AuditLogDetailsVm
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Username { get; set; }
        public AuditAction Action { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string CorrelationId { get; set; }
        public string IPAddress { get; set; }
        public string RequestPath { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
    }
}
