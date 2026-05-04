using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Sops;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Services;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using MassTransit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace CleanOpsAi.Modules.TaskOperations.UnitTests.Services
{
    public class TaskAssignmentServiceTests
    {
        private readonly ITaskAssignmentRepository _taskAssignmentRepo;
        private readonly ITaskStepExecutionRepository _taskStepExecutionRepo;
        private readonly IMapper _mapper;
        private readonly IRecurrenceExpander _expander;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IIdGenerator _idGenerator;
        private readonly IRequestClient<SopStepsRequested> _sopStepsClient;
        private readonly IRequestClient<GetWorkersByIdsRequest> _workerClient;
        private readonly IUserContext _userContext;
        private readonly IRequestClient<GetWorkersByWorkAreaRequest> _workerByAreaClient;

        private readonly TaskAssignmentService _service;

        public TaskAssignmentServiceTests()
        {
            _taskAssignmentRepo = Substitute.For<ITaskAssignmentRepository>();
            _taskStepExecutionRepo = Substitute.For<ITaskStepExecutionRepository>();
            _mapper = Substitute.For<IMapper>();
            _expander = Substitute.For<IRecurrenceExpander>();
            _dateTimeProvider = Substitute.For<IDateTimeProvider>();
            _idGenerator = Substitute.For<IIdGenerator>();
            _sopStepsClient = Substitute.For<IRequestClient<SopStepsRequested>>();
            _workerClient = Substitute.For<IRequestClient<GetWorkersByIdsRequest>>();
            _userContext = Substitute.For<IUserContext>();
            _workerByAreaClient = Substitute.For<IRequestClient<GetWorkersByWorkAreaRequest>>();

            _service = new TaskAssignmentService(
                _taskAssignmentRepo,
                _taskStepExecutionRepo,
                _mapper,
                _expander,
                _dateTimeProvider,
                _idGenerator,
                _sopStepsClient,
                _workerClient,
                _userContext,
                _workerByAreaClient
            );
        }

        // =========================
        // DELETE SUCCESS
        // =========================
        [Fact]
        public async Task Delete_ShouldMarkAsDeleted()
        {
            var taskId = Guid.NewGuid();
            var assignment = new TaskAssignment { Id = taskId, IsDeleted = false };

            _taskAssignmentRepo.GetByIdAsync(taskId).Returns(assignment);

            var result = await _service.Delete(taskId);

            Assert.True(result);
            Assert.True(assignment.IsDeleted);
            await _taskAssignmentRepo.Received(1).SaveChangesAsync();
        }

		// =========================
		// DELETE FAIL - NOT FOUND
		// =========================
		[Fact]
		public async Task Delete_ShouldThrowNotFound_WhenNotFound()
		{
			var taskId = Guid.NewGuid();

			_taskAssignmentRepo.GetByIdAsync(taskId)
				.Returns((TaskAssignment?)null);

			await Assert.ThrowsAsync<NotFoundException>(() =>
				_service.Delete(taskId));
		}

		// =========================
		// GET BY ID SUCCESS
		// =========================
		[Fact]
        public async Task GetById_ShouldReturnDto()
        {
            var taskId = Guid.NewGuid();
            var assignment = new TaskAssignment
            {
                Id = taskId,
                Status = TaskAssignmentStatus.NotStarted
            };

            _taskAssignmentRepo.GetByIdExist(taskId, default).Returns(assignment);

            var dto = new TaskAssignmentDto { Id = taskId };
            _mapper.Map<TaskAssignmentDto>(assignment).Returns(dto);

            var result = await _service.GetById(taskId);

            Assert.NotNull(result);
            Assert.Equal(taskId, result.Id);
        }

		// =========================
		// GET BY ID FAIL - NOT FOUND
		// =========================
		[Fact]
		public async Task GetById_ShouldThrowNotFound_WhenNotFound()
		{
			var taskId = Guid.NewGuid();

			_taskAssignmentRepo.GetByIdExist(taskId, default)
				.Returns((TaskAssignment?)null);

			await Assert.ThrowsAsync<NotFoundException>(() =>
				_service.GetById(taskId));
		}

		// =========================
		// UPDATE SUCCESS
		// =========================
		[Fact]
		public async Task Update_ShouldUpdateFields_WhenValidRequest()
		{
			var taskId = Guid.NewGuid();
			var assigneeId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			var assignment = new TaskAssignment
			{
				Id = taskId,
				Status = TaskAssignmentStatus.NotStarted,
				IsAdhocTask = false,
				TaskName = "Old Task Name",
				DisplayLocation = "Old Location",
				AssigneeId = Guid.NewGuid(),
				AssigneeName = "Old Assignee",
				ScheduledStartAt = now,
				ScheduledEndAt = now.AddMinutes(60)
			};

			var dto = new TaskAssignmentUpdateDto
			{
				TaskName = "New Task Name",
				DisplayLocation = "New Location",
				AssigneeId = assigneeId,
				AssigneeName = "New Assignee",
				ScheduledStartAt = now.AddHours(1),
				DurationMinutes = 90
			};

			var expectedDto = new TaskAssignmentDto { Id = taskId };

			_taskAssignmentRepo.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(assignment);
			_taskAssignmentRepo.HasOverlapAsync(
				assigneeId, Arg.Any<DateTime>(), Arg.Any<DateTime>(),
				taskId, Arg.Any<CancellationToken>()).Returns(false);
			_mapper.Map<TaskAssignmentDto>(assignment).Returns(expectedDto);

			var result = await _service.Update(taskId, dto);

			Assert.NotNull(result);
			Assert.Equal(taskId, result.Id);
			Assert.Equal("New Task Name", assignment.TaskName);
			Assert.Equal("New Location", assignment.DisplayLocation);
			Assert.Equal(assigneeId, assignment.AssigneeId);
			Assert.Equal("New Assignee", assignment.AssigneeName);
			Assert.Equal(now.AddHours(1), assignment.ScheduledStartAt);
			Assert.Equal(now.AddHours(1).AddMinutes(90), assignment.ScheduledEndAt);
			await _taskAssignmentRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		// =========================
		// UPDATE FAILURE - NOT FOUND
		// =========================
		[Fact]
		public async Task Update_ShouldThrowNotFoundException_WhenTaskNotFound()
		{
			var taskId = Guid.NewGuid();
			_taskAssignmentRepo.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns((TaskAssignment?)null);

			await Assert.ThrowsAsync<NotFoundException>(() =>
				_service.Update(taskId, new TaskAssignmentUpdateDto()));
		}

		// =========================
		// UPDATE FAILURE - NOT STARTED
		// =========================
		[Theory]
		[InlineData(TaskAssignmentStatus.InProgress)]
		[InlineData(TaskAssignmentStatus.Completed)]
		[InlineData(TaskAssignmentStatus.Block)]
		public async Task Update_ShouldThrowBadRequest_WhenStatusIsNotNotStarted(TaskAssignmentStatus status)
		{
			var taskId = Guid.NewGuid();
			var assignment = new TaskAssignment
			{
				Id = taskId,
				Status = status,
				IsAdhocTask = false
			};

			_taskAssignmentRepo.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(assignment);

			var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
				_service.Update(taskId, new TaskAssignmentUpdateDto()));

			Assert.Contains("NotStarted", ex.Message);
		}

		// =========================
		// UPDATE FAILURE - ADHOC TASK
		// =========================
		[Fact]
		public async Task Update_ShouldThrowBadRequest_WhenTaskIsAdhoc()
		{
			var taskId = Guid.NewGuid();
			var assignment = new TaskAssignment
			{
				Id = taskId,
				Status = TaskAssignmentStatus.NotStarted,
				IsAdhocTask = true
			};

			_taskAssignmentRepo.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(assignment);

			var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
				_service.Update(taskId, new TaskAssignmentUpdateDto()));

			Assert.Contains("Adhoc", ex.Message);
		}

		// =========================
		// UPDATE FAILURE - ASSIGNEE WITHOUT NAME
		// =========================
		[Fact]
		public async Task Update_ShouldThrowBadRequest_WhenAssigneeIdWithoutAssigneeName()
		{
			var taskId = Guid.NewGuid();
			var assignment = new TaskAssignment
			{
				Id = taskId,
				Status = TaskAssignmentStatus.NotStarted,
				IsAdhocTask = false
			};

			var dto = new TaskAssignmentUpdateDto
			{
				AssigneeId = Guid.NewGuid(),
				AssigneeName = null  
			};

			_taskAssignmentRepo.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(assignment);

			await Assert.ThrowsAsync<BadRequestException>(() =>
				_service.Update(taskId, dto));
		}

		// =========================
		// UPDATE FAILURE - ASSIGNEE NAME WITHOUT ID
		// =========================
		[Fact]
		public async Task Update_ShouldThrowBadRequest_WhenAssigneeNameWithoutAssigneeId()
		{
			var taskId = Guid.NewGuid();
			var assignment = new TaskAssignment
			{
				Id = taskId,
				Status = TaskAssignmentStatus.NotStarted,
				IsAdhocTask = false
			};

			var dto = new TaskAssignmentUpdateDto
			{
				AssigneeId = null, // thiếu id
				AssigneeName = "Some Name"
			};

			_taskAssignmentRepo.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(assignment);

			await Assert.ThrowsAsync<BadRequestException>(() =>
				_service.Update(taskId, dto));
		}

		// =========================
		// UPDATE FAILURE - DURATION INVALID
		// =========================
		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		[InlineData(-100)]
		public async Task Update_ShouldThrowBadRequest_WhenDurationMinutesIsInvalid(int duration)
		{
			var taskId = Guid.NewGuid();
			var assignment = new TaskAssignment
			{
				Id = taskId,
				Status = TaskAssignmentStatus.NotStarted,
				IsAdhocTask = false,
				ScheduledStartAt = DateTime.UtcNow,
				ScheduledEndAt = DateTime.UtcNow.AddMinutes(60)
			};

			var dto = new TaskAssignmentUpdateDto { DurationMinutes = duration };

			_taskAssignmentRepo.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(assignment);

			var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
				_service.Update(taskId, dto));

			Assert.Contains("DurationMinutes", ex.Message);
		}

		// =========================
		// UPDATE FAILURE - OVERLAP
		// =========================
		[Fact]
		public async Task Update_ShouldThrowBadRequest_WhenAssigneeHasOverlap()
		{
			var taskId = Guid.NewGuid();
			var assigneeId = Guid.NewGuid();
			var now = DateTime.UtcNow;

			var assignment = new TaskAssignment
			{
				Id = taskId,
				Status = TaskAssignmentStatus.NotStarted,
				IsAdhocTask = false,
				AssigneeId = assigneeId,
				ScheduledStartAt = now,
				ScheduledEndAt = now.AddMinutes(60)
			};

			_taskAssignmentRepo.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(assignment);
			_taskAssignmentRepo.HasOverlapAsync(
				assigneeId, Arg.Any<DateTime>(), Arg.Any<DateTime>(),
				taskId, Arg.Any<CancellationToken>()).Returns(true);

			var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
				_service.Update(taskId, new TaskAssignmentUpdateDto()));

			Assert.Contains("already has a task", ex.Message);
		}

		// =========================
		// UPDATE SUCCESS - ONLY START TIME 
		// =========================
		[Fact]
		public async Task Update_ShouldKeepOldDuration_WhenOnlyStartTimeUpdated()
		{
			var taskId = Guid.NewGuid();
			var now = DateTime.UtcNow;
			var oldDuration = TimeSpan.FromMinutes(60);

			var assignment = new TaskAssignment
			{
				Id = taskId,
				Status = TaskAssignmentStatus.NotStarted,
				IsAdhocTask = false,
				AssigneeId = Guid.NewGuid(),
				ScheduledStartAt = now,
				ScheduledEndAt = now + oldDuration
			};

			var newStart = now.AddHours(2);
			var dto = new TaskAssignmentUpdateDto { ScheduledStartAt = newStart };

			_taskAssignmentRepo.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(assignment);
			_taskAssignmentRepo.HasOverlapAsync(
				Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(),
				taskId, Arg.Any<CancellationToken>()).Returns(false);
			_mapper.Map<TaskAssignmentDto>(assignment).Returns(new TaskAssignmentDto { Id = taskId });

			await _service.Update(taskId, dto);

			Assert.Equal(newStart, assignment.ScheduledStartAt);
			Assert.Equal(newStart + oldDuration, assignment.ScheduledEndAt); 
		}


		// =========================
		// UPDATE STATUS SUCCESS
		// =========================
		[Fact]
		public async Task UpdateStatus_ShouldUpdateStatus_WhenTaskExists()
		{
			var taskId = Guid.NewGuid();
			var assignment = new TaskAssignment
			{
				Id = taskId,
				Status = TaskAssignmentStatus.NotStarted
			};

			_taskAssignmentRepo.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
				.Returns(assignment);

			var result = await _service.UpdateStatus(taskId, TaskAssignmentStatus.InProgress);

			Assert.True(result);
			Assert.Equal(TaskAssignmentStatus.InProgress, assignment.Status);
			await _taskAssignmentRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		// =========================
		// UPDATE STATUS FAIL - NOT FOUND
		// =========================
		[Fact]
		public async Task UpdateStatus_ShouldThrowNotFoundException_WhenTaskNotFound()
		{
			var taskId = Guid.NewGuid();
			_taskAssignmentRepo.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
				.Returns((TaskAssignment?)null);

			await Assert.ThrowsAsync<NotFoundException>(() =>
				_service.UpdateStatus(taskId, TaskAssignmentStatus.InProgress));
		}

		// =========================
		// GETS - PAGINATION SUCCESS
		// =========================
		[Fact]
        public async Task Gets_ShouldReturnPaginatedResult()
        {
            var filter = new TaskAssignmentFilter();
            var paginationRequest = new PaginationRequest(1, 10);

            var assignments = new List<TaskAssignment>
            {
                new TaskAssignment { Id = Guid.NewGuid() },
                new TaskAssignment { Id = Guid.NewGuid() }
            };

            var paginatedResult = new PaginatedResult<TaskAssignment>(
                1, 10, 2, assignments);

			_taskAssignmentRepo.Gets(filter, paginationRequest, Arg.Any<CancellationToken>())
	            .Returns(paginatedResult);

			var dtos = new List<TaskAssignmentDto>
            {
                new TaskAssignmentDto { Id = assignments[0].Id },
                new TaskAssignmentDto { Id = assignments[1].Id }
            };

			_mapper.Map<List<TaskAssignmentDto>>(Arg.Any<List<TaskAssignment>>())
	            .Returns(dtos);

			var result = await _service.Gets(filter, paginationRequest);

            Assert.NotNull(result);
            Assert.Equal(2, result.Content.Count);
            Assert.Equal(2, result.TotalElements);
        }

        // =========================
        // START TASK SUCCESS - ADHOC
        // =========================
        [Fact]
        public async Task StartTaskAsync_ShouldStartAdhocTask()
        {
            var taskId = Guid.NewGuid();
            var workerId = Guid.NewGuid();

            var assignment = new TaskAssignment
            {
                Id = taskId,
                AssigneeId = workerId,
                IsAdhocTask = true,
                Status = TaskAssignmentStatus.NotStarted
            };

            _taskAssignmentRepo.GetByIdAsync(taskId, default).Returns(assignment);
            _dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);

            var result = await _service.StartTaskAsync(taskId, workerId);

            Assert.NotNull(result);
            Assert.Equal(taskId, result.TaskAssignmentId);
            Assert.Equal(TaskAssignmentStatus.InProgress, result.Status);
            Assert.True(result.IsAdhoc);
            Assert.Empty(result.Steps);
            await _taskAssignmentRepo.Received(1).SaveChangesAsync(default);
        }

        // =========================
        // START TASK SUCCESS - REGULAR WITH STEPS
        // =========================
        [Fact]
        public async Task StartTaskAsync_ShouldStartRegularTaskWithSteps()
        {
            var taskId = Guid.NewGuid();
            var workerId = Guid.NewGuid();
            var taskScheduleId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            _dateTimeProvider.UtcNow.Returns(now);
            _idGenerator.Generate().Returns(Guid.NewGuid());

            var assignment = new TaskAssignment
            {
                Id = taskId,
                AssigneeId = workerId,
                TaskScheduleId = taskScheduleId,
                IsAdhocTask = false,
                Status = TaskAssignmentStatus.NotStarted
            };

            _taskAssignmentRepo.GetByIdAsync(taskId, default).Returns(assignment);
            _taskStepExecutionRepo.ExistsByAssignmentId(taskId, default).Returns(false);

            var sopSteps = new List<SopStepMetadataDto>
            {
                new SopStepMetadataDto { Id = Guid.NewGuid(), StepOrder = 1, IsDeleted = false, ConfigSchema = "{}", ConfigDetail = "{}" },
                new SopStepMetadataDto { Id = Guid.NewGuid(), StepOrder = 2, IsDeleted = false, ConfigSchema = "{}", ConfigDetail = "{}" }
            };

            var sopMetadata = JsonSerializer.Serialize(sopSteps);

            var sopResponse = Substitute.For<Response<SopStepsIntegrated>>();
            sopResponse.Message.Returns(new SopStepsIntegrated
            {
                Found = true,
                Metadata = sopMetadata
            });

            _sopStepsClient.GetResponse<SopStepsIntegrated>(Arg.Any<SopStepsRequested>(), Arg.Any<CancellationToken>())
                .Returns(sopResponse);

            var result = await _service.StartTaskAsync(taskId, workerId);

            Assert.NotNull(result);
            Assert.Equal(taskId, result.TaskAssignmentId);
            Assert.Equal(TaskAssignmentStatus.InProgress, result.Status);
            Assert.False(result.IsAdhoc);
            Assert.Equal(2, result.Steps.Count);
            await _taskAssignmentRepo.Received(1).SaveChangesAsync(default);
            await _taskStepExecutionRepo.Received(1).AddRangeStepExecutionsAsync(Arg.Any<List<TaskStepExecution>>(), default);
        }

		
		[Fact]
		public async Task StartTaskAsync_ShouldThrow_WhenTaskNotFound()
		{
			var taskId = Guid.NewGuid();
			var workerId = Guid.NewGuid();

			_taskAssignmentRepo
				.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
				.Returns((TaskAssignment?)null);

			await Assert.ThrowsAsync<NotFoundException>(() =>
				_service.StartTaskAsync(taskId, workerId));
		}
         
		// =========================
		[Fact]
        public async Task StartTaskAsync_ShouldThrow_WhenWrongWorker()
        {
            var taskId = Guid.NewGuid();
            var workerId = Guid.NewGuid();
            var assignedWorkerId = Guid.NewGuid();

            var assignment = new TaskAssignment
            {
                Id = taskId,
                AssigneeId = assignedWorkerId,
                IsAdhocTask = true
            };

            _taskAssignmentRepo.GetByIdAsync(taskId, default).Returns(assignment);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                _service.StartTaskAsync(taskId, workerId));
        }

        
        [Fact]
        public async Task StartTaskAsync_ShouldThrow_WhenAlreadyInProgress()
        {
            var taskId = Guid.NewGuid();
            var workerId = Guid.NewGuid();

            var assignment = new TaskAssignment
            {
                Id = taskId,
                AssigneeId = workerId,
                IsAdhocTask = true,
                Status = TaskAssignmentStatus.InProgress
            };

            _taskAssignmentRepo.GetByIdAsync(taskId, default).Returns(assignment);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.StartTaskAsync(taskId, workerId));
        }

        
        [Fact]
        public async Task CompleteTaskAsync_ShouldCompleteAdhocTask()
        {
            var taskId = Guid.NewGuid();
            var workerId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            _dateTimeProvider.UtcNow.Returns(now);

            var assignment = new TaskAssignment
            {
                Id = taskId,
                AssigneeId = workerId,
                IsAdhocTask = true,
                Status = TaskAssignmentStatus.InProgress
            };

            _taskAssignmentRepo.GetByIdAsync(taskId, default).Returns(assignment);

            var dto = new TaskCompletedDto { WorkerId = workerId };

            var result = await _service.CompleteTaskAsync(taskId, dto);

            Assert.NotNull(result);
            Assert.Equal(TaskAssignmentStatus.Completed, result.Status);
            await _taskAssignmentRepo.Received(1).SaveChangesAsync(default);
        }


		[Fact]
		public async Task CompleteTaskAsync_ShouldThrow_WhenTaskNotFound()
		{
			var taskId = Guid.NewGuid();

			_taskAssignmentRepo
				.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
				.Returns((TaskAssignment?)null);

			var dto = new TaskCompletedDto { WorkerId = Guid.NewGuid() };

			await Assert.ThrowsAsync<NotFoundException>(() =>
				_service.CompleteTaskAsync(taskId, dto));
		}


		[Fact]
        public async Task CompleteTaskAsync_ShouldThrow_WhenWrongWorker()
        {
            var taskId = Guid.NewGuid();
            var workerId = Guid.NewGuid();
            var assignedWorkerId = Guid.NewGuid();

            var assignment = new TaskAssignment
            {
                Id = taskId,
                AssigneeId = assignedWorkerId,
                Status = TaskAssignmentStatus.InProgress,
                IsAdhocTask = true
            };

            _taskAssignmentRepo.GetByIdAsync(taskId, default).Returns(assignment);

            var dto = new TaskCompletedDto { WorkerId = workerId };

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                _service.CompleteTaskAsync(taskId, dto));
        }

      
        [Fact]
        public async Task CompleteTaskAsync_ShouldThrow_WhenNotInProgress()
        {
            var taskId = Guid.NewGuid();
            var workerId = Guid.NewGuid();

            var assignment = new TaskAssignment
            {
                Id = taskId,
                AssigneeId = workerId,
                Status = TaskAssignmentStatus.NotStarted,
                IsAdhocTask = true
            };

            _taskAssignmentRepo.GetByIdAsync(taskId, default).Returns(assignment);

            var dto = new TaskCompletedDto { WorkerId = workerId };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CompleteTaskAsync(taskId, dto));
        }

        [Fact]
        public async Task CompleteTaskAsync_ShouldThrow_WhenOrderedTaskHasUnfinishedSteps()
        {
            var taskId = Guid.NewGuid();
            var workerId = Guid.NewGuid();

            var assignment = new TaskAssignment
            {
                Id = taskId,
                AssigneeId = workerId,
                Status = TaskAssignmentStatus.InProgress,
                IsAdhocTask = false
            };

            _taskAssignmentRepo.GetByIdAsync(taskId, default).Returns(assignment);
            _taskStepExecutionRepo.AnyUnfinishedStepAsync(taskId, default).Returns(true);

            var dto = new TaskCompletedDto { WorkerId = workerId };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CompleteTaskAsync(taskId, dto));
        }

       
        [Fact]
        public async Task CreateAdhocTask_ShouldCreateTask_WhenNoOverlap()
        {
            var workAreaId = Guid.NewGuid();
            var assigneeId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            _dateTimeProvider.UtcNow.Returns(now);
            _idGenerator.Generate().Returns(Guid.NewGuid());
            _userContext.UserId.Returns(Guid.NewGuid());

            var dto = new CreateAdhocTaskDto
            {
                WorkAreaId = workAreaId,
                AssigneeId = assigneeId,
                StartAt = now.AddHours(1),
                DurationMinutes = 60
            };

            var workerResponse = Substitute.For<Response<GetWorkersByWorkAreaResponse>>();
            workerResponse.Message.Returns(new GetWorkersByWorkAreaResponse
            {
                Workers = new List<WorkerDto> { new WorkerDto { Id = assigneeId } }
            });

            _workerByAreaClient.GetResponse<GetWorkersByWorkAreaResponse>(Arg.Any<GetWorkersByWorkAreaRequest>(), Arg.Any<CancellationToken>())
                .Returns(workerResponse);

            var workerDetailResponse = Substitute.For<Response<GetWorkersByIdsResponse>>();
            workerDetailResponse.Message.Returns(new GetWorkersByIdsResponse
            {
                Workers = new List<WorkerDto> { new WorkerDto { Id = assigneeId, FullName = "Worker A" } }
            });

            _workerClient.GetResponse<GetWorkersByIdsResponse>(Arg.Any<GetWorkersByIdsRequest>(), Arg.Any<CancellationToken>())
                .Returns(workerDetailResponse);

			_taskAssignmentRepo.GetOverlappingTask(assigneeId, dto.StartAt, Arg.Any<DateTime>()).Returns((TaskAssignment?)null);

			var createdDto = new TaskAssignmentDto { Id = Guid.NewGuid() };
            _mapper.Map<TaskAssignmentDto>(Arg.Any<TaskAssignment>()).Returns(createdDto);

            var result = await _service.CreateAdhocTask(dto);

            Assert.NotNull(result);
            await _taskAssignmentRepo.Received(1).InsertAsync(Arg.Any<TaskAssignment>(), default);
        }

       
        [Fact]
        public async Task CreateAdhocTask_ShouldThrow_WhenStartTimeInPast()
        {
            var now = DateTime.UtcNow;
            _dateTimeProvider.UtcNow.Returns(now);

            var dto = new CreateAdhocTaskDto
            {
                WorkAreaId = Guid.NewGuid(),
                AssigneeId = Guid.NewGuid(),
                StartAt = now.AddHours(-1),
                DurationMinutes = 60
            };

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateAdhocTask(dto));
        }

        // =========================
        // CREATE ADHOC TASK FAIL - WORKER NOT IN AREA
        // =========================
        [Fact]
        public async Task CreateAdhocTask_ShouldThrow_WhenWorkerNotInWorkArea()
        {
            var workAreaId = Guid.NewGuid();
            var assigneeId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            _dateTimeProvider.UtcNow.Returns(now);

            var dto = new CreateAdhocTaskDto
            {
                WorkAreaId = workAreaId,
                AssigneeId = assigneeId,
                StartAt = now.AddHours(1),
                DurationMinutes = 60
            };

            var workerResponse = Substitute.For<Response<GetWorkersByWorkAreaResponse>>();
            workerResponse.Message.Returns(new GetWorkersByWorkAreaResponse
            {
                Workers = new List<WorkerDto> { new WorkerDto { Id = Guid.NewGuid() } }
            });

            _workerByAreaClient.GetResponse<GetWorkersByWorkAreaResponse>(Arg.Any<GetWorkersByWorkAreaRequest>(), Arg.Any<CancellationToken>())
                .Returns(workerResponse);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateAdhocTask(dto));
        }

        // =========================
        // CREATE ADHOC TASK WITH OVERLAPPING - PAUSE OVERLAPPING TASK
        // =========================
        [Fact]
        public async Task CreateAdhocTask_ShouldPauseOverlappingTask()
        {
            var workAreaId = Guid.NewGuid();
            var assigneeId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            _dateTimeProvider.UtcNow.Returns(now);
            _idGenerator.Generate().Returns(Guid.NewGuid());
            _userContext.UserId.Returns(Guid.NewGuid());

            var dto = new CreateAdhocTaskDto
            {
                WorkAreaId = workAreaId,
                AssigneeId = assigneeId,
                StartAt = now.AddHours(1),
                DurationMinutes = 60
            };

            var workerResponse = Substitute.For<Response<GetWorkersByWorkAreaResponse>>();
            workerResponse.Message.Returns(new GetWorkersByWorkAreaResponse
            {
                Workers = new List<WorkerDto> { new WorkerDto { Id = assigneeId } }
            });

            _workerByAreaClient.GetResponse<GetWorkersByWorkAreaResponse>(Arg.Any<GetWorkersByWorkAreaRequest>(), Arg.Any<CancellationToken>())
                .Returns(workerResponse);

            var workerDetailResponse = Substitute.For<Response<GetWorkersByIdsResponse>>();
            workerDetailResponse.Message.Returns(new GetWorkersByIdsResponse
            {
                Workers = new List<WorkerDto> { new WorkerDto { Id = assigneeId, FullName = "Worker A" } }
            });

            _workerClient.GetResponse<GetWorkersByIdsResponse>(Arg.Any<GetWorkersByIdsRequest>(), Arg.Any<CancellationToken>())
                .Returns(workerDetailResponse);

            var overlappingTask = new TaskAssignment
            {
                Id = Guid.NewGuid(),
                Status = TaskAssignmentStatus.NotStarted
            };

            _taskAssignmentRepo.GetOverlappingTask(assigneeId, dto.StartAt, Arg.Any<DateTime>())
                .Returns(overlappingTask);

            var createdDto = new TaskAssignmentDto { Id = Guid.NewGuid() };
            _mapper.Map<TaskAssignmentDto>(Arg.Any<TaskAssignment>()).Returns(createdDto);

            var result = await _service.CreateAdhocTask(dto);

            Assert.NotNull(result);
            Assert.Equal(TaskAssignmentStatus.Block, overlappingTask.Status);
            await _taskAssignmentRepo.Received(1).InsertAsync(Arg.Any<TaskAssignment>(), default);
        }

		// =========================
		// GET WORKERS AVAILABLE BY AREA SUCCESS
		// =========================
		[Fact]
		public async Task GetWorkersAvailableByAreaAsync_ShouldGetBusyWorkers()
		{
			var workAreaId = Guid.NewGuid();
			var start = DateTime.UtcNow.AddHours(1);
			var end = DateTime.UtcNow.AddHours(2);

			var busyWorkerIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

			// mock worker service
			var workerResponse = Substitute.For<Response<GetWorkersByWorkAreaResponse>>();
			workerResponse.Message.Returns(new GetWorkersByWorkAreaResponse
			{
				Workers = new List<WorkerDto>
		{
			new WorkerDto { Id = busyWorkerIds[0], FullName = "A" },
			new WorkerDto { Id = Guid.NewGuid(), FullName = "B" }
		}
			});

			_workerByAreaClient
	            .GetResponse<GetWorkersByWorkAreaResponse>(
		            Arg.Any<GetWorkersByWorkAreaRequest>(),
		            Arg.Any<CancellationToken>(),
		            Arg.Any<RequestTimeout>())
	            .Returns(workerResponse);

			// mock repo
			_taskAssignmentRepo
				.GetBusyWorkerIdsAsync(workAreaId, start, end, Arg.Any<CancellationToken>())
				.Returns(busyWorkerIds);

			var result = await _service.GetWorkersAvailableByAreaAsync(workAreaId, start, end);

			Assert.NotNull(result);
		}
	}
}