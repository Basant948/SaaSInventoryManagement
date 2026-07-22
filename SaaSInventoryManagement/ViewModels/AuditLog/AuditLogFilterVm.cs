using SaaSInventoryManagement.Enums;

namespace SaaSInventoryManagement.ViewModels.AuditLog
{
    public class AuditLogFilterVm
    {
        public string? EntityName { get; set; }
        public AuditAction? Action { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
    }
}
