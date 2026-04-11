using CleanOpsAi.BuildingBlocks.Application.Common;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Response;

namespace CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Services
{
	public interface IWorkareaCheckinPointService
	{
		Task<WorkareaCheckinPointDto> GetByIdAsync(Guid id);

		Task<WorkareaCheckinPointDto> Create(WorkareaCheckinPointCreateDto request, CancellationToken ct = default);

		Task<WorkareaCheckinPointDto> Update(Guid id, WorkareaCheckinPointUpdateDto request, CancellationToken ct = default);

		Task<bool> Delete(Guid id, CancellationToken ct = default);

		Task<Result> Activate(Guid id, CancellationToken ct = default);
		Task<Result> Deactivate(Guid id, CancellationToken ct = default);   
	}
}
