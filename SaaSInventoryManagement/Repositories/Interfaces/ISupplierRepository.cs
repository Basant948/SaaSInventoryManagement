using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Repositories.Interfaces
{
    public interface ISupplierRepository : IGenericRepository<Supplier>
    {
        Task<bool> NameExistsAsync(string name, int? excludingId = null);
    }
}
