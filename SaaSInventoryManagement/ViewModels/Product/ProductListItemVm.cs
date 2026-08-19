namespace SaaSInventoryManagement.ViewModels.Product
{
    public class ProductListItemVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public string? Image { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
