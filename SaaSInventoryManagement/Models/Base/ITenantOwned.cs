namespace SaaSInventoryManagement.Models.Base
{
    public interface ITenantOwned
    {
        int TenantId { get; set; }
    }
}
