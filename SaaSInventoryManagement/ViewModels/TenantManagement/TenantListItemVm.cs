namespace SaaSInventoryManagement.ViewModels.TenantManagement
{
    public class TenantListItemVm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserCount { get; set; }
    }
}
