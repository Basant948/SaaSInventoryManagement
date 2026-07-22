namespace SaaSInventoryManagement.ViewModels.AuditLog
{
    public class AuditLogIndexVm
    {
        public AuditLogFilterVm Filter { get; set; } = new();
        public List<AuditLogListItemVm> Items { get; set; } = new();
        public List<string> EntityNames { get; set; } = new();
        public int PageSize { get; set; } = 25;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
