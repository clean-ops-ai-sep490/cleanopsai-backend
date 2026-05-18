using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Workers; 

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IWorkerService
    {
        Task<List<WorkerResponse>> GetByIdAsync(Guid id);

        Task<WorkerResponse> GetByUserIdAsync(Guid userId);

        Task<List<WorkerResponse>> GetAllAsync();

        Task<PagedResponse<WorkerResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<WorkerResponse> CreateAsync(WorkerCreateRequest request);

        Task<WorkerResponse> UpdateAsync(Guid id, WorkerUpdateRequest request);

        Task<int> DeleteAsync(Guid id);

        Task<List<WorkerResponse>> GetInforAsync();

        Task<List<WorkerResponse>> FilterAsync(WorkerFilterRequest request);

		Task<List<WorkerDto>> GetWorkersByIds(List<Guid> ids);

		Task<List<WorkerByUserIdResponse>> GetWorkersByUserIds(List<Guid> userIds);

        Task<List<Guid>> GetWorkersWithAllSkillsAndCertsAsync(List<Guid> workerIds, List<Guid> requiredSkillIds, List<Guid> requiredCertIds, CancellationToken ct);

		Task<List<Guid>> GetQualifiedWorkersAsync(
			List<Guid> requiredSkillIds,
			List<Guid> requiredCertificationIds,
			CancellationToken ct = default);

		Task<bool> IsWorkerQualifiedAsync(
			Guid workerId,
			List<Guid> requiredSkillIds,
			List<Guid> requiredCertificationIds,
			CancellationToken ct = default);

        Task<List<WorkerResponse>> NlpFilterAsync(string? query, CancellationToken cancellationToken = default);

    }
}
