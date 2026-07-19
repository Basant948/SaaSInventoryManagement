using SaaSInventoryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace SaaSInventoryManagement.Data.Seeding
{
    public static class PermissionSeeder
    {
        private const string SeedKey = "permissions:v1";

        private static readonly PermDef[] Permissions =
        {
            // General 
            new("inv.dashboard.view",        "Dashboard",           "General",       "fa-solid fa-gauge",              "dashboard",         1),

            // Catalog 
            new("inv.products.view",         "View Products",       "Catalog",       "fa-solid fa-box",                "products",          10),
            new("inv.products.manage",       "Manage Products",     "Catalog",       "fa-solid fa-box-open",           "products-manage",   11),
            new("inv.categories.view",       "View Categories",     "Catalog",       "fa-solid fa-tags",               "categories",        12),
            new("inv.categories.manage",     "Manage Categories",   "Catalog",       "fa-solid fa-tag",                "categories-manage", 13),

            // Inventory 
            new("inv.warehouses.view",       "View Warehouses",     "Inventory",     "fa-solid fa-warehouse",          "warehouses",        20),
            new("inv.warehouses.manage",     "Manage Warehouses",   "Inventory",     "fa-solid fa-industry",           "warehouses-manage", 21),
            new("inv.stock.view",            "View Stock Levels",   "Inventory",     "fa-solid fa-boxes-stacked",      "stock",             22),
            new("inv.stock.adjust",          "Adjust Stock",        "Inventory",     "fa-solid fa-sliders",            "stock-adjust",      23),
            new("inv.stock.transfer",        "Transfer Stock",      "Inventory",     "fa-solid fa-right-left",         "stock-transfer",    24),

            // Procurement
            new("inv.suppliers.view",        "View Suppliers",      "Procurement",   "fa-solid fa-truck",              "suppliers",         30),
            new("inv.suppliers.manage",      "Manage Suppliers",    "Procurement",   "fa-solid fa-truck-ramp-box",     "suppliers-manage",  31),
            new("inv.purchaseorders.view",   "View Purchase Orders","Procurement",   "fa-solid fa-file-invoice",       "purchase-orders",   32),
            new("inv.purchaseorders.create", "Create Purchase Order","Procurement",  "fa-solid fa-file-circle-plus",   "po-create",         33),
            new("inv.purchaseorders.approve","Approve Purchase Order","Procurement", "fa-solid fa-file-circle-check",  "po-approve",        34),

            //  Sales 
            new("inv.salesorders.view",      "View Sales Orders",   "Sales",         "fa-solid fa-receipt",            "sales-orders",      40),
            new("inv.salesorders.create",    "Create Sales Order",  "Sales",         "fa-solid fa-cart-plus",          "so-create",         41),

            // Reports 
            new("inv.reports.stock",         "Stock Report",        "Reports",       "fa-solid fa-chart-bar",          "reports-stock",     50),
            new("inv.reports.sales",         "Sales Report",        "Reports",       "fa-solid fa-chart-line",         "reports-sales",     51),
            new("inv.reports.purchase",      "Purchase Report",     "Reports",       "fa-solid fa-chart-pie",          "reports-purchase",  52),

            //  Administration 
            new("inv.users.manage",          "Manage Users",        "Administration","fa-solid fa-users-gear",         "users-manage",      60),
            new("inv.settings.manage",       "Company Settings",    "Administration","fa-solid fa-gear",               "settings",          61),
            new("inv.auditlogs.view",        "Audit Logs",          "Administration","fa-solid fa-clipboard-list",     "audit-logs",        62),
        };


        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (await db.SeedHistory.AnyAsync(s => s.SeedKey == SeedKey))
                return;

            var existingKeys = await db.Permissions.Select(p => p.Key).ToHashSetAsync();

            var toInsert = Permissions
                .Where(p => !existingKeys.Contains(p.Key))
                .Select(p => new Permission
                {
                    Key = p.Key,
                    DisplayName = p.DisplayName,
                    GroupName = p.GroupName,
                    IconClass = p.IconClass,
                    ControllerAction = p.ControllerAction,
                    SortOrder = p.SortOrder,
                    IsActive = true
                })
                .ToList();

            if (toInsert.Count > 0)
                db.Permissions.AddRange(toInsert);

            db.SeedHistory.Add(new Models.SeedHistory
            {
                SeedKey = SeedKey,
                AppliedAt = DateTime.UtcNow,
                Notes = $"Seeded {toInsert.Count} permission(s)"
            });

            await db.SaveChangesAsync();
        }

        private record PermDef(
            string Key,
            string DisplayName,
            string GroupName,
            string IconClass,
            string ControllerAction,
            int SortOrder);
    }
}
