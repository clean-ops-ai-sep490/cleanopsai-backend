using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Repositories
{
	public class TaskAssignmentRepository : BaseRepo<TaskAssignment, Guid>, ITaskAssignmentRepository
	{
		public TaskAssignmentRepository(TaskOperationsDbContext context) : base(context)
		{
			
		}

		public async Task<bool> ExistsAsync(Guid scheduleId, DateTime scheduledAt)
		{
			return await _context.TaskAssignments
				.AnyAsync(x =>
					x.TaskScheduleId == scheduleId &&
					x.ScheduledStartAt == scheduledAt &&
					!x.IsDeleted);
		}

		public async Task BulkInsertAsync(IEnumerable<TaskAssignment> assignments)
		{
			await _context.TaskAssignments.AddRangeAsync(assignments);
			await _context.SaveChangesAsync();
		}

		public async Task<TaskAssignment?> GetByIdExist(Guid id, CancellationToken ct)
		{
			return await _context.TaskAssignments.FirstOrDefaultAsync(x=>x.Id == id, ct);
		}

		public async Task<PaginatedResult<TaskAssignment>> GetSwapCandidatesAsync(Guid workAreaId,
			Guid excludeAssigneeId,
			DateTime scheduledStartAt,
			DateTime scheduledEndAt,
			DateTime weekStart,
			DateTime weekEnd,
			DateOnly? date,
			TimeOnly? preferredStartTime,
			PaginationRequest paginationRequest,
			CancellationToken ct = default)
		{
			var query = _context.TaskAssignments
				.Where(t =>
					t.WorkAreaId == workAreaId &&
					t.AssigneeId != excludeAssigneeId &&
					t.Status != TaskAssignmentStatus.NotStarted &&
					t.ScheduledStartAt < scheduledEndAt &&
					t.ScheduledEndAt > scheduledStartAt &&
					t.ScheduledStartAt >= weekStart &&
					t.ScheduledStartAt < weekEnd &&
					!t.TaskSwapRequests.Any(s =>
						s.Status == SwapRequestStatus.PendingTargetApproval ||
						s.Status == SwapRequestStatus.PendingManagerApproval)
				);

			if (date.HasValue)
				query = query.Where(t =>
					DateOnly.FromDateTime(t.ScheduledStartAt) == date.Value);

			if (preferredStartTime.HasValue)
				query = query.Where(t =>
					TimeOnly.FromDateTime(t.ScheduledStartAt) >= preferredStartTime.Value);

			return await query
				.OrderBy(t => t.ScheduledStartAt)
				.ToPaginatedResultAsync(paginationRequest, ct);
		}

		public async Task<bool> HasTimeConflictAsync(Guid excludeTaskId, Guid assigneeId, DateTime scheduledStartAt, DateTime scheduledEndAt, CancellationToken ct = default)
		{
			return await _context.TaskAssignments
				.Where(t =>
					t.Id != excludeTaskId &&          
					t.AssigneeId == assigneeId &&
					t.Status != TaskAssignmentStatus.Completed &&
					t.Status != TaskAssignmentStatus.Block &&
					t.ScheduledStartAt < scheduledEndAt &&   
					t.ScheduledEndAt > scheduledStartAt
				)
				.AnyAsync(ct);
		}

		public async Task<PaginatedResult<TaskAssignment>> Gets(TaskAssignmentFilter filter, PaginationRequest request, CancellationToken ct = default)
		{
			var query = _context.TaskAssignments.AsQueryable();

			if (filter.AssigneeId.HasValue)
				query = query.Where(x => x.AssigneeId == filter.AssigneeId.Value);

			if (filter.Status.HasValue)
				query = query.Where(x => x.Status == filter.Status.Value);

			if (filter.FromDate.HasValue)
				query = query.Where(x => x.ScheduledStartAt >= filter.FromDate.Value.Date);

			if (filter.ToDate.HasValue)
				query = query.Where(x => x.ScheduledEndAt <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));

			return await query.ToPaginatedResultAsync(request, ct);
		}
	}
}
