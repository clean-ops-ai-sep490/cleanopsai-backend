using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Response;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities;

namespace CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Services
{
	public interface IAccessDeviceService
	{

		Task<AccessDeviceDto> Create(AccessDeviceCreateDto request, CancellationToken ct = default);

		Task<AccessDeviceDto> GetById(Guid id, CancellationToken ct = default);

		Task<PaginatedResult<AccessDeviceDto>> GetByCheckinPointAsync(Guid checkinPointId, PaginationRequest request, CancellationToken ct = default);

		Task<AccessDeviceDto?> GetByIdentifierAsync(string identifier, CancellationToken ct = default);
	}
}
