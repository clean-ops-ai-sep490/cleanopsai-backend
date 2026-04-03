using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
    public interface IAdHocRequestService
    {
        Task<AdHocRequestDto?> GetById(Guid id, CancellationToken ct = default);

        Task<PaginatedResult<AdHocRequestDto>> Gets(PaginationRequest request, CancellationToken ct = default);

        Task<PaginatedResult<AdHocRequestDto>> GetsByWorkerId(Guid workerId, PaginationRequest request, CancellationToken ct = default);

        Task<PaginatedResult<AdHocRequestDto>> GetsByTaskAssignmentId(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default);

        Task<PaginatedResult<AdHocRequestDto>> GetsByStatus(AdHocRequestStatus status, PaginationRequest request, CancellationToken ct = default);

        Task<PaginatedResult<AdHocRequestDto>> GetsByType(AdHocRequestType type, PaginationRequest request, CancellationToken ct = default);

        Task<AdHocRequestDto?> Create(CreateAdHocRequestDto dto, CancellationToken ct = default);

        Task<AdHocRequestDto?> Update(Guid id, UpdateAdHocRequestDto dto, CancellationToken ct = default);

        Task<AdHocRequestDto?> Review(Guid id, ReviewAdHocRequestDto dto, CancellationToken ct = default);

        Task<bool> Delete(Guid id, CancellationToken ct = default);
    }
}
