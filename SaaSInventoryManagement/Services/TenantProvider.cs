using SaaSInventoryManagement.Services.Interfaces_;

namespace SaaSInventoryManagement.Services
{
    public class TenantProvider : ITenantProvider
    {
        public const string TenantIdClaimType = "tenant_id";

        private readonly IHttpContextAccessor _httpContextAccessor;

        private bool _resolved;
        private int? _tenantId;
        private bool _isSuperAdmin;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? TenantId
        {
            get
            {
                EnsureResolved();
                return _tenantId;
            }
        }

        public bool IsSuperAdmin
        {
            get
            {
                EnsureResolved();
                return _isSuperAdmin;
            }
        }

        private void EnsureResolved()
        {
            if (_resolved) return;

            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst(TenantIdClaimType);

            _tenantId = claim != null && int.TryParse(claim.Value, out var id) ? id : null;
            _isSuperAdmin = user?.IsInRole("SuperAdmin") ?? false;
            _resolved = true;
        }
    }
}
