using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Repositories.Interfaces
{
    public interface IWarehouseRepository : IGenericRepository<Warehouse>
    {
        Task<bool> CodeExistsAsync(string code, int? excludingId = null);
    }
}
