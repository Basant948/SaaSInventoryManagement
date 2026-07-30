namespace SaaSInventoryManagement.ViewModels.RoleManagement
{
    public class SavePermissionsRequestVm
    {
        public string UserId { get; set; }
        public List<string> PermissionKeys { get; set; } = new();
    }
}
