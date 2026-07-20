namespace SaaSInventoryManagement.ViewModels.Navigation
{
    public class NavItemDto
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string GroupName { get; set; }
        public string IconClass { get; set; }
        public string ControllerAction { get; set; }
        public int SortOrder { get; set; }
    }
}
