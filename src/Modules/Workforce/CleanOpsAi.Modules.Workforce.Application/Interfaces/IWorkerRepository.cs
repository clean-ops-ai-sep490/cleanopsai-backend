using CleanOpsAi.Modules.Workforce.Application.Dtos.Workers;
using CleanOpsAi.Modules.Workforce.Domain.Entities;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IWorkerRepository
    {
        Task<Worker?> GetByIdAsync(Guid id);

        Task<Worker?> GetByUserIdAsync(string userId);

        Task<List<Worker>> GetAllAsync();

        Task<(List<Worker> Items, int TotalCount)> GetAllPaginationAsync(
            int pageNumber,
            int pageSize);

        Task<int> CreateAsync(Worker worker);

        Task<int> UpdateAsync(Worker worker);

        Task<int> DeleteAsync(Guid id);

        Task<List<Worker>> FilterAsync(WorkerFilterRequest request);

        Task<List<Worker>> GetWorkersByIds(List<Guid> ids);

	}
}
