using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaSInventoryManagement.Services.Interfaces_;
using SaaSInventoryManagement.ViewModels.AuditLog;

namespace SaaSInventoryManagement.Controllers
{
    [Authorize(Policy = "perm:inv.auditlogs.view")]
    public class AuditLogController : Controller
    {
        private const int PageSize = 25;

        private readonly ApplicationDbContext _db;
        private readonly ITenantProvider _tenantProvider;

        public AuditLogController(ApplicationDbContext db, ITenantProvider tenantProvider)
        {
            _db = db;
            _tenantProvider = tenantProvider;
        }

        public async Task<IActionResult> Index(AuditLogFilterVm filter)
        {
            if (_tenantProvider.TenantId is null)
                return Forbid();

            var page = filter.Page < 1 ? 1 : filter.Page;

            var query = _db.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.EntityName))
                query = query.Where(a => a.EntityName == filter.EntityName);

            if (filter.Action is not null)
                query = query.Where(a => a.Action == filter.Action);

            if (filter.From is DateTime from)
                query = query.Where(a => a.CreatedAt >= from.Date);

            if (filter.To is DateTime to)
                query = query.Where(a => a.CreatedAt < to.Date.AddDays(1));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(a => new AuditLogListItemVm
                {
                    Id = a.Id,
                    CreatedAt = a.CreatedAt,
                    Username = a.Username,
                    Action = a.Action,
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    IPAddress = a.IPAddress
                })
                .ToListAsync();

            var entityNames = await _db.AuditLogs.AsNoTracking()
                .Select(a => a.EntityName)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();

            filter.Page = page;

            return View(new AuditLogIndexVm
            {
                Filter = filter,
                Items = items,
                EntityNames = entityNames,
                PageSize = PageSize,
                TotalCount = totalCount
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            if (_tenantProvider.TenantId is null)
                return Forbid();

            var entry = await _db.AuditLogs.AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AuditLogDetailsVm
                {
                    Id = a.Id,
                    CreatedAt = a.CreatedAt,
                    Username = a.Username,
                    Action = a.Action,
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    CorrelationId = a.CorrelationId,
                    IPAddress = a.IPAddress,
                    RequestPath = a.RequestPath,
                    OldValues = a.OldValues,
                    NewValues = a.NewValues
                })
                .FirstOrDefaultAsync();

            if (entry is null)
                return NotFound();

            return View(entry);
        }
    }
}
