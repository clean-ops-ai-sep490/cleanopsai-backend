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
    public interface IEmergencyLeaveRequestService
    {
        Task<EmergencyLeaveRequestDto?> GetById(Guid id, CancellationToken ct = default);
        Task<PaginatedResult<EmergencyLeaveRequestDto>> Gets(PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<EmergencyLeaveRequestDto>> GetsByWorkerId(Guid workerId, PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<EmergencyLeaveRequestDto>> GetsByTaskAssignmentId(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<EmergencyLeaveRequestDto>> GetsByStatus(RequestStatus status, PaginationRequest request, CancellationToken ct = default);
        Task<EmergencyLeaveRequestDto?> Create(CreateEmergencyLeaveRequestDto dto, CancellationToken ct = default);
        Task<EmergencyLeaveRequestDto?> Update(Guid id, UpdateEmergencyLeaveRequestDto dto, CancellationToken ct = default);
        Task<EmergencyLeaveRequestDto?> Review(Guid id, ReviewEmergencyLeaveRequestDto dto, CancellationToken ct = default);
        Task<bool> Delete(Guid id, CancellationToken ct = default);
    }
}
