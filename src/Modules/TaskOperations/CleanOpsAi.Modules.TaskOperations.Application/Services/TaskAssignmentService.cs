using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums; 

namespace CleanOpsAi.Modules.TaskOperations.Application.Services
{
	public class TaskAssignmentService : ITaskAssignmentService
	{
		private readonly ITaskAssignmentRepository _taskAssignmentRepository;
		private readonly IMapper _mapper;
		private readonly IRecurrenceExpander _expander;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IIdGenerator _idGenerator; 

		public TaskAssignmentService(ITaskAssignmentRepository taskAssignmentRepository,
			IMapper mapper,
			IRecurrenceExpander expander, IDateTimeProvider dateTimeProvider, IIdGenerator idGenerator)
		{
			_taskAssignmentRepository = taskAssignmentRepository;
			_mapper = mapper;
			_expander = expander;
			_dateTimeProvider = dateTimeProvider;
			_idGenerator = idGenerator;
		}


		public async Task<bool> Delete(Guid id)
		{
			var taskAssignment = await _taskAssignmentRepository.GetByIdAsync(id); 
			if (taskAssignment == null) return false;

			taskAssignment.IsDeleted = true; 
			await _taskAssignmentRepository.SaveChangesAsync(); 
			return true;
		}

		public async Task<TaskAssignmentDto?> GetById(Guid id, CancellationToken ct = default)
		{
			var taskAssignment = await _taskAssignmentRepository.GetByIdExist(id, ct);
			if (taskAssignment == null) return null;

			return _mapper.Map<TaskAssignmentDto?>(taskAssignment);
		}

		public async Task<PaginatedResult<TaskAssignmentDto>> Gets(PaginationRequest request, CancellationToken ct = default)
		{
			var result = await _taskAssignmentRepository.GetsPaging(request, ct);

			return new PaginatedResult<TaskAssignmentDto>(
				result.PageNumber,
				result.PageSize,
				result.TotalElements,
				_mapper.Map<List<TaskAssignmentDto>>(result.Content));
		}

		public async Task<PaginatedResult<TaskAssignmentDto>> GetsByAssigneeId(Guid assgineeId,PaginationRequest request, CancellationToken ct = default)
		{
			var result = await _taskAssignmentRepository.GetsByAssigneeIdPaging(assgineeId, request, ct);

			return new PaginatedResult<TaskAssignmentDto>(
				result.PageNumber,
				result.PageSize,
				result.TotalElements,
				_mapper.Map<List<TaskAssignmentDto>>(result.Content));
		}

		public async Task<TaskAssignmentDto?> Update(Guid id, TaskAssignmentDto dto)
		{
			var taskAssignment = await _taskAssignmentRepository.GetByIdAsync(id);

			if (taskAssignment == null)
				return null;

			_mapper.Map(dto, taskAssignment);
			taskAssignment.LastModified = DateTime.UtcNow;
			taskAssignment.LastModifiedBy = "admin-123";
			 
			await _taskAssignmentRepository.SaveChangesAsync();

			return _mapper.Map<TaskAssignmentDto>(taskAssignment); 
		}

		public async Task<bool> UpdateStatus(Guid id, TaskAssignmentStatus status)
		{
			var taskAssignment = await _taskAssignmentRepository.GetByIdAsync(id);
			if (taskAssignment == null) return false;

			taskAssignment.Status = status; 
			await _taskAssignmentRepository.SaveChangesAsync(); 
			return true;

		}

		public async Task GenerateAsync(GenerateTaskAssignmentsRequestedEvent msg)
		{
			var scheduledTimes = _expander.Expand(
					msg.RecurrenceType,
					msg.RecurrenceConfig,
					msg.FromDate,
					msg.ToDate);

			var toInsert = new List<TaskAssignment>();

			foreach (var scheduledAt in scheduledTimes)
			{
				if (await _taskAssignmentRepository.ExistsAsync(msg.ScheduleId, scheduledAt))
					continue;

				toInsert.Add(new TaskAssignment
				{
					Id = _idGenerator.Generate(),
					TaskScheduleId = msg.ScheduleId,
					AssigneeId = msg.AssigneeId ?? Guid.Empty,
					OriginalAssigneeId = msg.AssigneeId ?? Guid.Empty,
					WorkAreaId = msg.WorkAreaId,
					ScheduledStartAt = scheduledAt,
					ScheduledEndAt = scheduledAt.AddMinutes(msg.DurationMinutes),
					Status = TaskAssignmentStatus.NotStarted,
					IsAdhocTask = false,
					Created = _dateTimeProvider.UtcNow,
					CreatedBy = "system"
				});
			}

			if (toInsert.Count > 0)
				await _taskAssignmentRepository.BulkInsertAsync(toInsert);
		}
	}
}
