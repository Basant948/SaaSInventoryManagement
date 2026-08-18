using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Repositories.Interfaces
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<bool> NameExistsAsync(string name, int? excludingId = null);
    }
}
