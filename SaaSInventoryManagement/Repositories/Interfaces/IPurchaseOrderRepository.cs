using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Repositories.Interfaces
{
    public interface IPurchaseOrderRepository : IGenericRepository<PurchaseOrder>
    {
        Task<PurchaseOrder?> GetWithLinesAsync(int id);

        Task<bool> PoNumberExistsAsync(string poNumber);
    }
}
