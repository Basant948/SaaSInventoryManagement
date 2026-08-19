using System.Collections.ObjectModel;

namespace SaaSInventoryManagement.Infrastructure.Navigation
{
    public class FeaturePageRoutes
    {
            public static readonly ReadOnlyDictionary<string, (string Controller, string Action)> PageLinks =
        new(new Dictionary<string, (string Controller, string Action)>
        {
            ["inv.categories.view"] = ("Category", "Index"),
            ["inv.products.view"] = ("Product", "Index"),
            ["inv.warehouses.view"] = ("Warehouse", "Index"),
            ["inv.stock.view"] = ("Stock", "Index"),
            ["inv.stock.adjust"] = ("Stock", "Adjust"),
            ["inv.stock.transfer"] = ("Stock", "Transfer"),
            ["inv.suppliers.view"] = ("Supplier", "Index"),
            ["inv.purchaseorders.view"] = ("PurchaseOrder", "Index"),
            ["inv.purchaseorders.create"] = ("PurchaseOrder", "Create"),
            ["inv.customers.view"] = ("Customer", "Index"),
            ["inv.salesorders.view"] = ("SalesOrder", "Index"),
            ["inv.salesorders.create"] = ("SalesOrder", "Create"),
            ["inv.reports.stock"] = ("Report", "Stock"),
            ["inv.users.manage"] = ("RoleManagement", "Index"),
            ["inv.auditlogs.view"] = ("AuditLog", "Index"),
            ["inv.settings.manage"] = ("Settings", "Index"),
        });
        public static readonly IReadOnlySet<string> HiddenFromSidebar = new HashSet<string>
        {
            "inv.categories.manage",
            "inv.products.manage",
            "inv.warehouses.manage",
            "inv.suppliers.manage",
            "inv.purchaseorders.approve",
            "inv.customers.manage",
        };
    }
}
