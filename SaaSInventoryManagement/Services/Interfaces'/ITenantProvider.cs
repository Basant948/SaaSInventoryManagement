namespace SaaSInventoryManagement.Services.Interfaces_
{
    public interface ITenantProvider
    {
        int? TenantId { get; }
        bool IsSuperAdmin { get; }
    }
}
