namespace SaaSInventoryManagement.ViewModels.RoleManagement
{
    public class PermissionCheckItemVm
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string GroupName { get; set; }
        public bool Granted { get; set; }
    }
}
