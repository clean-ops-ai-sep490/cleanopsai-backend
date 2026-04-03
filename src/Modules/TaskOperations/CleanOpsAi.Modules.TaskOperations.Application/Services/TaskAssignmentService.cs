using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using MassTransit;
using System.Text.Json;

namespace CleanOpsAi.Modules.TaskOperations.Application.Services
{
	public class TaskAssignmentService : ITaskAssignmentService
	{
		private readonly ITaskAssignmentRepository _taskAssignmentRepository;
		private readonly ITaskStepExecutionRepository _taskStepExecutionRepository;
		private readonly IMapper _mapper;
		private readonly IRecurrenceExpander _expander;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IIdGenerator _idGenerator;
		private readonly IRequestClient<SopStepsRequested> _sopStepsClient;
		//private readonly IPublishEndpoint _publishEndpoint;

		public TaskAssignmentService(ITaskAssignmentRepository taskAssignmentRepository,
			ITaskStepExecutionRepository taskStepExecutionRepository,
			IMapper mapper,
			IRecurrenceExpander expander,
			IDateTimeProvider dateTimeProvider,
			IIdGenerator idGenerator,
			IRequestClient<SopStepsRequested> sopClient)
		{
			_taskAssignmentRepository = taskAssignmentRepository;
			_taskStepExecutionRepository = taskStepExecutionRepository;
			_mapper = mapper;
			_expander = expander;
			_dateTimeProvider = dateTimeProvider;
			_idGenerator = idGenerator;
			_sopStepsClient = sopClient;
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
			Console.WriteLine("assignee: " + msg.AssigneeName);
			Console.WriteLine("DisplayLocation: " + msg.DisplayLocation);

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
					AssigneeName = msg.AssigneeName!,
					OriginalAssigneeName = msg.AssigneeName!,
					DisplayLocation = msg.DisplayLocation!,
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

		public async Task<StartTaskDto> StartTaskAsync(Guid taskAssignmentId, Guid workerId, CancellationToken ct = default)
		{
			var assignment = await _taskAssignmentRepository.GetByIdAsync(taskAssignmentId, ct)
				?? throw new NotFoundException(nameof(TaskAssignment), taskAssignmentId);
			if (assignment.IsAdhocTask)
			{  
				if (assignment.AssigneeId != workerId)
					throw new ForbiddenException("Not your task");

				if (assignment.Status != TaskAssignmentStatus.NotStarted)
					throw new BadRequestException(
						$"Task was in status {assignment.Status}, can not start");

				assignment.Status = TaskAssignmentStatus.InProgress;
				assignment.LastModified = _dateTimeProvider.UtcNow;

				await _taskAssignmentRepository.SaveChangesAsync(ct);
				return new StartTaskDto
				{
					TaskAssignmentId = assignment.Id,
					Status = assignment.Status,
					Steps = new List<TaskStepExecutionDto>(),
					IsAdhoc = true
				};

			}

			if (assignment.AssigneeId != workerId)
				throw new ForbiddenException("Not your task");

			if (assignment.Status != TaskAssignmentStatus.NotStarted)
				throw new BadRequestException(
					$"Task was in status {assignment.Status}, can not start");

			if (await _taskStepExecutionRepository.ExistsByAssignmentId(assignment.Id, ct))
				throw new BadRequestException("Task already started");

			var response = await _sopStepsClient
				.GetResponse<SopStepsIntegrated>(
					new SopStepsRequested
					{
						TaskScheduleId = assignment.TaskScheduleId
					}, ct);

			if (!response.Message.Found || string.IsNullOrEmpty(response.Message.Metadata))
				throw new BadRequestException(
					$"Can not find sop step for TaskSchedule {assignment.TaskScheduleId}.");

			var sopSteps = JsonSerializer.Deserialize<List<SopStepMetadataDto>>(
				response.Message.Metadata,
				new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
				?? throw new BadRequestException("Metadata invalid");

			var validSteps = sopSteps
				.Where(s => !s.IsDeleted)
				.OrderBy(s => s.StepOrder)
				.ToList();

			if (!validSteps.Any())
				throw new BadRequestException("There are no valid steps in the SOP");

			assignment.Status = TaskAssignmentStatus.InProgress;
			assignment.LastModified = _dateTimeProvider.UtcNow;

			var stepExecutions = validSteps
				.Select((s, index) => new TaskStepExecution
				{
					Id = _idGenerator.Generate(),
					TaskAssignmentId = assignment.Id,
					SopStepId = s.Id,
					Status = index == 0
						? TaskStepExecutionStatus.InProgress
						: TaskStepExecutionStatus.NotStarted,
					ResultData = "{}",
					StepOrder = s.StepOrder,
					Created = _dateTimeProvider.UtcNow
				}).ToList();

			await _taskStepExecutionRepository.AddRangeStepExecutionsAsync(stepExecutions, ct);
			await _taskAssignmentRepository.SaveChangesAsync(ct);

			//publish

			var stepDtos = stepExecutions.Select((s, index) => new TaskStepExecutionDto
			{
				Id = s.Id,
				SopStepId = s.SopStepId,
				StepOrder = validSteps[index].StepOrder,
				Status = s.Status.ToString()
			}).ToList();

			return new StartTaskDto
			{
				TaskAssignmentId = assignment.Id,
				Status = assignment.Status,
				Steps = stepDtos,
				IsAdhoc = false
			};
		}

		public async Task<PaginatedResult<TaskAssignmentDto>> Gets(TaskAssignmentFilter filter, PaginationRequest request, CancellationToken ct = default)
		{
			var result = await _taskAssignmentRepository.Gets(filter, request, ct);

			return new PaginatedResult<TaskAssignmentDto>(
				result.PageNumber,
				result.PageSize,
				result.TotalElements,
				_mapper.Map<List<TaskAssignmentDto>>(result.Content));
		}

		public async Task<StartTaskDto> CompleteTaskAsync(Guid taskAssignmentId, TaskCompletedDto dto, CancellationToken ct = default)
		{
			var assignment = await _taskAssignmentRepository.GetByIdAsync(taskAssignmentId, ct)
				 ?? throw new NotFoundException(nameof(TaskAssignment), taskAssignmentId);

			if (assignment.AssigneeId != dto.WorkerId)
				throw new ForbiddenException("Not your task");

			if (assignment.Status != TaskAssignmentStatus.InProgress)
				throw new BadRequestException($"Task is in status {assignment.Status}, cannot complete");

			if (assignment.Status == TaskAssignmentStatus.Completed)
				throw new BadRequestException("Task already completed");

			if (!assignment.IsAdhocTask)
			{
				var hasUnfinishedStep = await _taskStepExecutionRepository
					.AnyUnfinishedStepAsync(taskAssignmentId, ct);

				if (hasUnfinishedStep)
					throw new BadRequestException("All steps must be completed before finishing task");
			}

			assignment.Status = TaskAssignmentStatus.Completed;
			assignment.LastModified = _dateTimeProvider.UtcNow;

			await _taskAssignmentRepository.SaveChangesAsync(ct);

			return new StartTaskDto
			{
				TaskAssignmentId = assignment.Id,
				Status = assignment.Status,
				Steps = new List<TaskStepExecutionDto>() 
			};
		}
	}
}
