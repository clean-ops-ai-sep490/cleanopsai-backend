using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data; 
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

		public async Task<IEnumerable<(Guid ScheduleId, DateTime ScheduledAt)>> GetExistingKeysAsync(
		List<Guid> scheduleIds)
		{
			return await _context.TaskAssignments
				.Where(x => scheduleIds.Contains(x.TaskScheduleId) && !x.IsDeleted)
				.Select(x => new { x.TaskScheduleId, x.ScheduledStartAt })
				.AsNoTracking() // read-only, không cần track
				.ToListAsync()
				.ContinueWith(t => t.Result.Select(x => (x.TaskScheduleId, x.ScheduledStartAt)));
		}

		public async Task BulkInsertAsync(IEnumerable<TaskAssignment> assignments)
		{
			await _context.TaskAssignments.AddRangeAsync(assignments);
			await _context.SaveChangesAsync();
		}

		public async Task<TaskAssignment?> GetByIdExist(Guid id, CancellationToken ct)
		{
			return await _context.TaskAssignments
				.Include(x => x.TaskStepExecutions)
				.FirstOrDefaultAsync(x=>x.Id == id, ct);
		}

		//public async Task<PaginatedResult<TaskAssignment>> GetSwapCandidatesAsync(Guid workAreaId,
		//	Guid excludeAssigneeId,
		//	DateTime scheduledStartAt,
		//	DateTime scheduledEndAt,
		//	DateTime weekStart,
		//	DateTime weekEnd,
		//	DateOnly? date,
		//	TimeOnly? preferredStartTime,
		//	List<Guid>? qualifiedWorkerIds,
		//	PaginationRequest paginationRequest,
		//	CancellationToken ct = default)
		//{
		//	var query = _context.TaskAssignments
		//		.Where(t =>
		//			t.WorkAreaId == workAreaId &&
		//			t.AssigneeId != excludeAssigneeId &&
		//			t.Status == TaskAssignmentStatus.NotStarted &&
		//			//t.ScheduledStartAt < scheduledEndAt &&
		//			//t.ScheduledEndAt > scheduledStartAt &&
		//			t.ScheduledStartAt >= weekStart &&
		//			t.ScheduledStartAt < weekEnd &&
		//			!t.TaskSwapRequests.Any(s =>
		//				s.Status == SwapRequestStatus.PendingTargetApproval ||
		//				s.Status == SwapRequestStatus.PendingSupervisorApproval)
		//		);

		//	if (qualifiedWorkerIds != null && qualifiedWorkerIds.Any())
		//		query = query.Where(t => qualifiedWorkerIds.Contains(t.AssigneeId));

		//	if (date.HasValue)
		//		query = query.Where(t =>
		//			DateOnly.FromDateTime(t.ScheduledStartAt) == date.Value);

		//	if (preferredStartTime.HasValue)
		//		query = query.Where(t =>
		//			TimeOnly.FromDateTime(t.ScheduledStartAt) >= preferredStartTime.Value);

		//	return await query
		//		.OrderBy(t => t.ScheduledStartAt)
		//		.ToPaginatedResultAsync(paginationRequest, ct);
		//}

		public async Task<PaginatedResult<TaskAssignment>> GetSwapCandidatesAsync(
		Guid requesterTaskId,           // Task A (của requester)
		Guid requesterAssigneeId,       // Worker A
		DateTime requesterScheduledStartAt,  // Giờ bắt đầu Task A
		DateTime requesterScheduledEndAt,    // Giờ kết thúc Task A
		Guid workAreaId,
		DateTime weekStart,
		DateTime weekEnd,
		DateTime cutoffTime,
		DateOnly? date,
		TimeOnly? preferredStartTime,
		List<Guid>? qualifiedWorkerIds,
		PaginationRequest paginationRequest,
		CancellationToken ct = default)
		{
			var query = _context.TaskAssignments
				.Where(t => 
					t.WorkAreaId == workAreaId && 
					t.AssigneeId != requesterAssigneeId && 
					t.Status == TaskAssignmentStatus.NotStarted && 
					t.ScheduledStartAt >= weekStart &&
					t.ScheduledStartAt < weekEnd &&
					t.ScheduledStartAt > cutoffTime &&
					// Điều kiện 1: Worker B không có task khác trùng giờ Task A
					// (vì sau swap, B sẽ làm Task A)
					!_context.TaskAssignments.Any(other =>
						other.AssigneeId == t.AssigneeId &&
						other.Id != t.Id &&
						other.ScheduledStartAt < requesterScheduledEndAt &&
						other.ScheduledEndAt > requesterScheduledStartAt) &&

					// Điều kiện 2: Worker A không có task khác trùng giờ Task B
					// (vì sau swap, A sẽ làm Task B)
					!_context.TaskAssignments.Any(other =>
						other.AssigneeId == requesterAssigneeId &&
						other.Id != requesterTaskId &&
						other.ScheduledStartAt < t.ScheduledEndAt &&
						other.ScheduledEndAt > t.ScheduledStartAt) &&

					// Điều kiện 3: Task B không có pending swap request nào
					!t.TaskSwapRequests.Any(s =>
						s.Status == SwapRequestStatus.PendingTargetApproval ||
						s.Status == SwapRequestStatus.PendingSupervisorApproval)
				);

			// Filter theo skills/certifications nếu SOP có yêu cầu
			if (qualifiedWorkerIds != null && qualifiedWorkerIds.Any())
				query = query.Where(t => qualifiedWorkerIds.Contains(t.AssigneeId));

			// Filter theo ngày cụ thể
			if (date.HasValue)
				query = query.Where(t => DateOnly.FromDateTime(t.ScheduledStartAt) == date.Value);

			// Filter theo giờ bắt đầu mong muốn
			if (preferredStartTime.HasValue)
				query = query.Where(t => TimeOnly.FromDateTime(t.ScheduledStartAt) >= preferredStartTime.Value);

			return await query
				.OrderBy(t => t.ScheduledStartAt)
				.ToPaginatedResultAsync(paginationRequest, ct);
		}

		public async Task<bool> HasOverlapAsync(
			Guid assigneeId,
			DateTime newStart,
			DateTime newEnd,
			Guid? excludeTaskId = null,
			CancellationToken ct = default)
		{
			return await _context.TaskAssignments
				.AnyAsync(x =>
					x.AssigneeId == assigneeId &&
					!x.IsDeleted && 
					x.Status != TaskAssignmentStatus.Completed &&
					(excludeTaskId == null || x.Id != excludeTaskId) &&
					x.ScheduledStartAt < newEnd &&
					x.ScheduledEndAt > newStart,
					ct);
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

			//if (filter.FromDate.HasValue)
			//	query = query.Where(x => x.ScheduledStartAt >= filter.FromDate.Value.Date);

			//if (filter.ToDate.HasValue)
			//	query = query.Where(x => x.ScheduledEndAt <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));

			if (filter.FromDate.HasValue)
			{
				var from = filter.FromDate.Value; 
				query = query.Where(x => x.ScheduledEndAt >= from);
			}

			if (filter.ToDate.HasValue)
			{
				var to = filter.ToDate.Value; 
				query = query.Where(x => x.ScheduledStartAt <= to);
			}

			if (filter.IsAdhocTask.HasValue)
				query = query.Where(x => x.IsAdhocTask == filter.IsAdhocTask.Value);

			return await query.ToPaginatedResultAsync(request, ct);
		}

        public async Task<TaskAssignment?> GetOverlappingTask(
			Guid assigneeId,
			DateTime start,
			DateTime end,
			CancellationToken ct = default)
        {
            return await _context.TaskAssignments
                .Where(t =>
                    t.AssigneeId == assigneeId &&
                    !t.IsDeleted &&
                    t.Status != TaskAssignmentStatus.Completed &&
                    t.ScheduledStartAt < end &&
                    t.ScheduledEndAt > start
                )
                .OrderBy(t => t.ScheduledStartAt)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<Guid>> GetBusyWorkerIdsAsync(
			Guid workAreaId,
			DateTime start,
			DateTime end,
			CancellationToken ct = default)
        {
            var startUtc = NormalizeToUtc(start);
            var endUtc = NormalizeToUtc(end);

            return await _context.TaskAssignments
                .Where(t =>
                    !t.IsDeleted &&
                    t.WorkAreaId == workAreaId &&   // filter theo khu vực
                    t.ScheduledStartAt < endUtc &&
                    t.ScheduledEndAt > startUtc
                )
                .Select(t => t.AssigneeId)
                .Distinct()
                .ToListAsync(ct);
        }

        public async Task<List<Guid>> GetBusyWorkerIdsWithoutAreaAsync(
			DateTime start,
			DateTime end,
			CancellationToken ct = default)
        {
            var startUtc = NormalizeToUtc(start);
            var endUtc = NormalizeToUtc(end);

            return await _context.TaskAssignments
                .Where(t =>
                    !t.IsDeleted &&
                    t.ScheduledStartAt < endUtc &&
                    t.ScheduledEndAt > startUtc
                )
                .Select(t => t.AssigneeId)
                .Distinct()
                .ToListAsync(ct);
        }

        private static DateTime NormalizeToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        public async Task<List<TaskAssignment>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default)
        {
            return await _context.TaskAssignments
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(ct);
        }

        public async Task<List<TaskAssignment>> GetTasksByWorkerAndDateRange(
			Guid workerId,
			DateTime from,
			DateTime to,
			CancellationToken ct)
        {
            return await _context.TaskAssignments
                .Where(x =>
                    x.AssigneeId == workerId &&
                    !x.IsDeleted &&
                    x.ScheduledStartAt < to &&
                    x.ScheduledEndAt > from // overlap
                )
                .ToListAsync(ct);
        }

        public async Task<int> CountAllAsync()
        {
            return await _context.TaskAssignments.CountAsync(x => !x.IsDeleted);
        }

        public async Task<int> CountByStatusAsync(TaskAssignmentStatus status)
        {
            return await _context.TaskAssignments
                .CountAsync(x => !x.IsDeleted && x.Status == status);
        }

        public async Task<List<WorkerTaskStatsDto>> GetTopWorkersByMonthAsync(
            DateTime from,
            DateTime to,
            bool descending,
            int take = 5)
        {
            var query = _context.TaskAssignments
                .Where(x =>
                    !x.IsDeleted &&
                    x.Status == TaskAssignmentStatus.Completed &&
                    x.ScheduledStartAt >= from &&
                    x.ScheduledStartAt <= to)
                .GroupBy(x => new { x.AssigneeId, x.AssigneeName })
                .Select(g => new WorkerTaskStatsDto
                {
                    WorkerId = g.Key.AssigneeId,
                    WorkerName = g.Key.AssigneeName,
                    TotalTasks = g.Count()
                });

            query = descending
                ? query.OrderByDescending(x => x.TotalTasks).ThenBy(x => x.WorkerName)
                : query.OrderBy(x => x.TotalTasks).ThenBy(x => x.WorkerName);

            return await query.Take(take).ToListAsync();
        }

		public async Task<PaginatedResult<TaskAssignment>> GetAdhocTasksCreateBySupervisor(string suppervisorId, PaginationRequest request, CancellationToken ct = default)
		{
			var query = _context.TaskAssignments.AsQueryable();

			query = query.Where(x => x.IsAdhocTask == true && x.CreatedBy == suppervisorId);

			return await query.ToPaginatedResultAsync(request, ct);
		}

	}
}
