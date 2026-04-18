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
    public interface IIssueReportService
    {
        Task<IssueReportDto?> GetById(Guid id, CancellationToken ct = default);
        Task<PaginatedResult<IssueReportDto>> Gets(PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<IssueReportDto>> GetsByWorkerId(Guid workerId, PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<IssueReportDto>> GetsByTaskAssignmentId(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<IssueReportDto>> GetsByStatus(IssueStatus status, PaginationRequest request, CancellationToken ct = default);
        Task<IssueReportDto?> Create(CreateIssueReportDto dto, CancellationToken ct = default);
        Task<IssueReportDto?> Update(Guid id, UpdateIssueReportDto dto, CancellationToken ct = default);
        Task<IssueReportDto?> Resolve(Guid id, ResolveIssueReportDto dto, CancellationToken ct = default);
        Task<bool> Delete(Guid id, CancellationToken ct = default);
    }
}
