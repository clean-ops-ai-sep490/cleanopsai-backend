using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Sops;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
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
        private readonly IRequestClient<GetWorkersByIdsRequest> _workerClient;
        private readonly IUserContext _userContext;
        private readonly IRequestClient<GetWorkersByWorkAreaRequest> _workerByAreaClient;

        public TaskAssignmentService(ITaskAssignmentRepository taskAssignmentRepository,
			ITaskStepExecutionRepository taskStepExecutionRepository,
			IMapper mapper,
			IRecurrenceExpander expander,
			IDateTimeProvider dateTimeProvider,
			IIdGenerator idGenerator,
			IRequestClient<SopStepsRequested> sopClient,
			IRequestClient<GetWorkersByIdsRequest> workerClient,
            IUserContext userContext,
            IRequestClient<GetWorkersByWorkAreaRequest> workerByAreaClient)
		{
			_taskAssignmentRepository = taskAssignmentRepository;
			_taskStepExecutionRepository = taskStepExecutionRepository;
			_mapper = mapper;
			_expander = expander;
			_dateTimeProvider = dateTimeProvider;
			_idGenerator = idGenerator;
			_sopStepsClient = sopClient;
			_workerClient = workerClient;
            _userContext = userContext;
			_workerByAreaClient = workerByAreaClient;
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

			return _mapper.Map<TaskAssignmentDto>(taskAssignment);
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
			var scheduleIds = msg.Items.Select(x => x.ScheduleId).Distinct().ToList();

			var existingKeys = await _taskAssignmentRepository.GetExistingKeysAsync(scheduleIds);

			var existingSet = new HashSet<(Guid, DateTime)>(existingKeys);
			 
			var toInsert = new List<TaskAssignment>();

			foreach (var item in msg.Items)
			{
				var scheduledTimes = _expander.Expand(
					item.RecurrenceType,
					item.RecurrenceConfig,
					item.FromDate,
					item.ToDate);

				foreach (var scheduledAt in scheduledTimes)
				{
					if (existingSet.Contains((item.ScheduleId, scheduledAt)))
						continue;

					toInsert.Add(new TaskAssignment
					{
						Id = _idGenerator.Generate(),
						TaskScheduleId = item.ScheduleId,
						AssigneeId = item.AssigneeId ?? Guid.Empty,
						OriginalAssigneeId = item.AssigneeId ?? Guid.Empty,
						AssigneeName = item.AssigneeName!,
						OriginalAssigneeName = item.AssigneeName!,
						DisplayLocation = item.DisplayLocation!,
						WorkAreaId = item.WorkAreaId,
						ScheduledStartAt = scheduledAt,
						ScheduledEndAt = scheduledAt.AddMinutes(item.DurationMinutes),
						Status = TaskAssignmentStatus.NotStarted,
						IsAdhocTask = false,
						Created = _dateTimeProvider.UtcNow,
						CreatedBy = "system"
					});
				}
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
					ConfigSnapshot = JsonSerializer.Serialize(new
					{
						schema = JsonSerializer.Deserialize<object>(s.ConfigSchema),
						detail = JsonSerializer.Deserialize<object>(s.ConfigDetail)
					}),
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
				ConfigSnapshot = JsonSerializer.Deserialize<JsonElement>(s.ConfigSnapshot),
				ResultData = JsonSerializer.Deserialize<JsonElement>(s.ResultData),
				Status = s.Status
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

        // create adhoc task without schedule, step
        public async Task<TaskAssignmentDto> CreateAdhocTask(CreateAdhocTaskDto dto)
        {
            var now = _dateTimeProvider.UtcNow;

            if (dto.StartAt < now)
                throw new BadRequestException("Không thể tạo task trong quá khứ");

            //  CHECK worker có thuộc work area không
            var workerInAreaResponse = await _workerByAreaClient
                .GetResponse<GetWorkersByWorkAreaResponse>(
                    new GetWorkersByWorkAreaRequest
                    {
                        WorkAreaId = dto.WorkAreaId
                    });

            var workersInArea = workerInAreaResponse.Message.Workers ?? new List<WorkerDto>();

            var isValidWorker = workersInArea.Any(x => x.Id == dto.AssigneeId);

            if (!isValidWorker)
                throw new BadRequestException("Worker không thuộc work area này");

            // Lấy tên assignee qua RabbitMQ
            var workerResponse = await _workerClient.GetResponse<GetWorkersByIdsResponse>(
                new GetWorkersByIdsRequest { WorkerIds = new List<Guid> { dto.AssigneeId } });

            var worker = workerResponse.Message.Workers.FirstOrDefault()
                ?? throw new BadRequestException("Không tìm thấy thông tin worker");

            var start = dto.StartAt;
            var end = dto.StartAt.AddMinutes(dto.DurationMinutes);

            //  tìm task bị overlap
            var overlappingTask = await _taskAssignmentRepository
                .GetOverlappingTask(dto.AssigneeId, start, end);

            //  nếu có → pause
            if (overlappingTask != null)
            {
                if (overlappingTask.Status == TaskAssignmentStatus.InProgress ||
                    overlappingTask.Status == TaskAssignmentStatus.NotStarted)
                {
                    overlappingTask.Status = TaskAssignmentStatus.Block;
                    overlappingTask.LastModified = now;
                    overlappingTask.LastModifiedBy = _userContext.UserId.ToString();
                }
                else
                {
                    throw new BadRequestException("Task không thể bị gián đoạn");
                }
            }

            //  tạo adhoc task
            var task = new TaskAssignment
            {
                Id = _idGenerator.Generate(),

                TaskScheduleId = _idGenerator.Generate(),

                AssigneeId = dto.AssigneeId,
                OriginalAssigneeId = dto.AssigneeId,

                AssigneeName = worker.FullName,
                OriginalAssigneeName = worker.FullName,

                WorkAreaId = dto.WorkAreaId,
                DisplayLocation = dto.DisplayLocation,

                ScheduledStartAt = start,
                ScheduledEndAt = end,

                Status = TaskAssignmentStatus.NotStarted,
                IsAdhocTask = true,
                NameAdhocTask = dto.Name,

                Created = now,
                CreatedBy = _userContext.UserId.ToString()
            };

            await _taskAssignmentRepository.InsertAsync(task);
            await _taskAssignmentRepository.SaveChangesAsync();

            return _mapper.Map<TaskAssignmentDto>(task);
        }

        public async Task<List<WorkerAvailabilityDto>> GetWorkersAvailableByAreaAsync(
			Guid workAreaId,
			DateTime start,
			DateTime end,
			CancellationToken ct = default)
        {
            try
            {
                var workerResponse = await _workerByAreaClient
                    .GetResponse<GetWorkersByWorkAreaResponse>(
                        new GetWorkersByWorkAreaRequest
                        {
                            WorkAreaId = workAreaId
                        },
                        timeout: RequestTimeout.After(s: 10), // thêm timeout rõ ràng
                        cancellationToken: ct);

                var workers = workerResponse.Message.Workers ?? new List<WorkerDto>();

                var busyWorkerIds = await _taskAssignmentRepository
                    .GetBusyWorkerIdsAsync(workAreaId, start, end, ct);

                var busySet = new HashSet<Guid>(busyWorkerIds);

                return workers
                    .Select(w => new WorkerAvailabilityDto
                    {
                        WorkerId = w.Id,
                        FullName = w.FullName,
                        IsBusy = busySet.Contains(w.Id)
                    })
                    .OrderBy(x => x.IsBusy)
                    .ThenBy(x => x.FullName)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR CALL WORKER SERVICE: " + ex.ToString());
                throw;
            }
        }

    }
}
