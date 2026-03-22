using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using static MassTransit.ValidationResultExtensions;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
	public interface ITaskSwapRequestService
	{
		Task<SwapRequestDto> CreateSwapRequestAsync(TaskSwapRequestCreateDto dto, Guid requesterId, CancellationToken ct = default);

		Task<PaginatedResult<SwapCandidateDto>> GetSwapCandidatesAsync(GetSwapCandidatesDto dto, PaginationRequest paginationRequest, CancellationToken ct = default);

		Task<Result> RespondSwapRequestAsync(
		RespondSwapRequestDto dto, Guid responderId, CancellationToken ct = default); 

		// Manager duyệt
		Task<Result> ReviewSwapRequestAsync(ReviewSwapRequestDto dto, Guid reviewerId, CancellationToken ct = default);

		// A hủy
		Task<Result> CancelSwapRequestAsync(Guid swapRequestId, Guid requesterId, CancellationToken ct = default);
	}
}
