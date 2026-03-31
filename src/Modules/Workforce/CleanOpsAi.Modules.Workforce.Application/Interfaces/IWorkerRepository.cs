using CleanOpsAi.Modules.Workforce.Application.Dtos.Workers;
using CleanOpsAi.Modules.Workforce.Domain.Entities;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IWorkerRepository
    {
        Task<Worker?> GetByIdAsync(Guid id);

        Task<Worker?> GetByUserIdAsync(Guid userId);

        Task<List<Worker>> GetAllAsync();

        Task<(List<Worker> Items, int TotalCount)> GetAllPaginationAsync(
            int pageNumber,
            int pageSize);

        Task<int> CreateAsync(Worker worker);

        Task<int> UpdateAsync(Worker worker);

        Task<int> DeleteAsync(Guid id);

        Task<List<Worker>> FilterAsync(WorkerFilterRequest request);

        Task<List<Worker>> GetWorkersByIds(List<Guid> ids);

        Task<List<Guid>> GetWorkersWithAllSkillsAndCertsAsync(
           List<Guid> workerIds,
           List<Guid> requiredSkillIds,
           List<Guid> requiredCertIds,
           CancellationToken ct);


		Task<List<Guid>> GetQualifiedWorkersAsync(
	        List<Guid> requiredSkillIds,
	        List<Guid> requiredCertificationIds,
	        CancellationToken ct = default);

		Task<bool> IsWorkerQualifiedAsync(
			Guid workerId,
			List<Guid> requiredSkillIds,
			List<Guid> requiredCertificationIds,
			CancellationToken ct = default);
	}
}
