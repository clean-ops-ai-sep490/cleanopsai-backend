using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories
{
    public interface IIssueReportRepository : IBaseRepo<IssueReport, Guid>
    {
        Task<IssueReport?> GetByIdExistAsync(Guid id, CancellationToken ct = default);
        Task<PaginatedResult<IssueReport>> GetsPagingAsync(PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<IssueReport>> GetsByWorkerIdPagingAsync(Guid workerId, PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<IssueReport>> GetsByTaskAssignmentIdPagingAsync(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default);
        Task<PaginatedResult<IssueReport>> GetsByStatusPagingAsync(IssueStatus status, PaginationRequest request, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid taskAssignmentId, Guid reportedByWorkerId, CancellationToken ct = default);
        Task AddAsync(IssueReport issueReport, CancellationToken ct = default);
        Task UpdateAsync(IssueReport issueReport, CancellationToken ct = default);
        Task DeleteAsync(IssueReport issueReport, CancellationToken ct = default);
    }
}
