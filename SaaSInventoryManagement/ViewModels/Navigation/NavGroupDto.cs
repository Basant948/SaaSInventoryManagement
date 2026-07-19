namespace SaaSInventoryManagement.ViewModels.Navigation
{
    public class NavGroupDto
    {
        public string GroupName { get; set; }
        public List<NavItemDto> Items { get; set; } = new();
    }
}
