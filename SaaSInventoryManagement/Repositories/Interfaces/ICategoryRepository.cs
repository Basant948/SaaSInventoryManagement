using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Repositories.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<bool> NameExistsAsync(string name, int? excludingId = null);
    }
}
