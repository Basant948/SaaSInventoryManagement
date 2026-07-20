namespace SaaSInventoryManagement.ViewModels.RoleManagement
{
    public class UserPermissionsResponseVm
    {
        public bool IsWildcard { get; set; }
        public List<PermissionCheckItemVm> Permissions { get; set; } = new();
    }
}
