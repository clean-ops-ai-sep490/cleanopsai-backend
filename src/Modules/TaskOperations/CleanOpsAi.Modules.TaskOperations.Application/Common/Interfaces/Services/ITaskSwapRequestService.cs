using CleanOpsAi.BuildingBlocks.Application.Common;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response; 

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
	public interface ITaskSwapRequestService
	{
		Task<Result<SwapRequestDto?>> GetById(Guid id, CancellationToken ct = default);

		Task<Result<SwapRequestDto>> CreateSwapRequestAsync(TaskSwapRequestCreateDto dto, Guid userId, CancellationToken ct = default);

		Task<Result<PaginatedResult<SwapCandidateDto>>> GetSwapCandidatesAsync(GetSwapCandidatesDto dto, PaginationRequest paginationRequest, CancellationToken ct = default);

		//Target worker confirm
		Task<Result> RespondSwapRequestAsync(RespondSwapRequestDto dto, CancellationToken ct = default); 

		// Manager duyệt
		Task<Result> ReviewSwapRequestAsync(ReviewSwapRequestDto dto, CancellationToken ct = default);

		// A hủy
		Task<Result> CancelSwapRequestAsync(Guid swapRequestId, Guid requesterId, CancellationToken ct = default);

		Task<PaginatedResult<SwapRequestDto>> GetSwapRequestsAsync(GetSwapRequestsDto dto, PaginationRequest paginationRequest ,CancellationToken ct = default);
	}
}
