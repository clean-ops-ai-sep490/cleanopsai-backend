using CleanOpsAi.Modules.ClientManagement.Domain.Entities; 

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface ILocationRepository
    {
        Task<Location?> GetByIdAsync(Guid id);
        Task<List<Location>> GetAllAsync();
        Task<(List<Location> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<int> CreateAsync(Location location);
        Task<int> UpdateAsync(Location location);
        Task<int> DeleteAsync(Guid id);
        Task<List<Location>> GetByClientIdAsync(Guid clientId);
        Task<(List<Location> Items, int TotalCount)> GetByClientIdPaginationAsync(Guid clientId, int pageNumber, int pageSize);

        Task<int> SoftDeleteAsync(Guid id);
	}
}
