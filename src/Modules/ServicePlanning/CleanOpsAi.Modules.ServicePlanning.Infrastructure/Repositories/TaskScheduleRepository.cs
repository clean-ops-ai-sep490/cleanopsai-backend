using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Repositories
{
	public class TaskScheduleRepository : BaseRepo<TaskSchedule, Guid>, ITaskScheduleRepository
	{
		public TaskScheduleRepository(ServicePlanningDbContext context) : base(context)
		{
		}

		public async Task<IReadOnlyList<TaskSchedule>> GetActiveSchedulesAsync()
		{
			return await _context.TaskSchedules
			.Where(x => x.IsActive == true) 
			.ToListAsync();
		}

		public async Task<TaskSchedule?> GetById(Guid id, CancellationToken cancellationToken = default)
		{
			return await _context.TaskSchedules.FirstOrDefaultAsync(s => s.Id == id && s.IsActive == true, cancellationToken);
		}

		public async Task<List<TaskSchedule>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default)
		{
			return await _context.TaskSchedules
			.Where(x => ids.Contains(x.Id) && x.IsActive == true)
			.ToListAsync(ct);
		}

		public async Task<IReadOnlyList<TaskSchedule>> GetConflictingCandidateSchedulesAsync(
			Guid? workAreaDetailId,
			Guid slaShiftId,
			Guid? assigneeId,
			DateOnly windowStart,
			DateOnly windowEnd,
			Guid? excludeScheduleId = null,
			CancellationToken cancellationToken = default)
		{
			var query = _context.TaskSchedules
				.Where(x => x.IsActive == true)
				.Where(x => x.SlaShiftId == slaShiftId)
				.Where(x => x.ContractStartDate <= windowEnd)
				.Where(x => x.ContractEndDate == null || x.ContractEndDate >= windowStart);

			if (workAreaDetailId.HasValue)
				query = query.Where(x => x.WorkAreaDetailId == workAreaDetailId.Value);
			else
				query = query.Where(x => x.WorkAreaDetailId == null);

			if (excludeScheduleId.HasValue)
				query = query.Where(x => x.Id != excludeScheduleId.Value);

			if (assigneeId.HasValue)
				query = query.Where(x => x.AssigneeId == assigneeId);

			return await query.ToListAsync(cancellationToken);
		}

		public async Task<PaginatedResult<TaskSchedule>> GetsPaging(GetsTaskScheduleQuery query, PaginationRequest request, CancellationToken ct = default)
		{
			var taskSchedules = _context.TaskSchedules.AsQueryable();

			if (!string.IsNullOrWhiteSpace(query.Name))
			{
				taskSchedules = taskSchedules.Where(x =>
					EF.Functions.ILike(x.Name, $"%{query.Name}%")
				);
			}

			taskSchedules = query.IsDescending ? taskSchedules.OrderByDescending(x => x.Name) : taskSchedules.OrderBy(x => x.Name);

			return await taskSchedules.ToPaginatedResultAsync(request, ct);
		}
	}
}
