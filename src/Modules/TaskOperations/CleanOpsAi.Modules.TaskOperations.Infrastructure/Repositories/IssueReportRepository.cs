using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Repositories
{
    public class IssueReportRepository : BaseRepo<IssueReport, Guid>, IIssueReportRepository
    {
        public IssueReportRepository(TaskOperationsDbContext context) : base(context)
        {
        }

        public async Task<IssueReport?> GetByIdExistAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.IssueReports
                .Include(x => x.TaskAssignment)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        }

        public async Task<PaginatedResult<IssueReport>> GetsPagingAsync(PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.IssueReports
                .Include(x => x.TaskAssignment)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.Created)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<IssueReport>> GetsByWorkerIdPagingAsync(Guid workerId, PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.IssueReports
                .Include(x => x.TaskAssignment)
                .Where(x => x.ReportedByWorkerId == workerId && !x.IsDeleted)
                .OrderByDescending(x => x.Created)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<IssueReport>> GetsByTaskAssignmentIdPagingAsync(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.IssueReports
                .Include(x => x.TaskAssignment)
                .Where(x => x.TaskAssignmentId == taskAssignmentId && !x.IsDeleted)
                .OrderByDescending(x => x.Created)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<PaginatedResult<IssueReport>> GetsByStatusPagingAsync(IssueStatus status, PaginationRequest request, CancellationToken ct = default)
        {
            return await _context.IssueReports
                .Include(x => x.TaskAssignment)
                .Where(x => x.Status == status && !x.IsDeleted)
                .OrderByDescending(x => x.Created)
                .ToPaginatedResultAsync(request, ct);
        }

        public async Task<bool> ExistsAsync(Guid taskAssignmentId, Guid reportedByWorkerId, CancellationToken ct = default)
        {
            return await _context.IssueReports
                .AnyAsync(x =>
                    x.TaskAssignmentId == taskAssignmentId &&
                    x.ReportedByWorkerId == reportedByWorkerId &&
                    !x.IsDeleted, ct);
        }

        public async Task AddAsync(IssueReport issueReport, CancellationToken ct = default)
        {
            await _context.IssueReports.AddAsync(issueReport, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(IssueReport issueReport, CancellationToken ct = default)
        {
            _context.IssueReports.Update(issueReport);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(IssueReport issueReport, CancellationToken ct = default)
        {
            issueReport.IsDeleted = true;
            _context.IssueReports.Update(issueReport);
            await _context.SaveChangesAsync(ct);
        }
    }
}