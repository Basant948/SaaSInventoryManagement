using SaaSInventoryManagement.Models;

namespace SaaSInventoryManagement.Repositories.Interfaces
{
    public interface ISalesOrderRepository : IGenericRepository<SalesOrder>
    {
        Task<SalesOrder?> GetWithLinesAsync(int id);

        Task<bool> SoNumberExistsAsync(string soNumber);
    }
}
