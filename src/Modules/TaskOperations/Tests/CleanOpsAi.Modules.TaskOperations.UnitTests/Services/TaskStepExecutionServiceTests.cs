using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Services;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace CleanOpsAi.Modules.TaskOperations.UnitTests.Services
{
    public class TaskStepExecutionServiceTests
    {
        private readonly ITaskStepExecutionRepository _repository;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IMapper _mapper; 
        private readonly TaskStepExecutionService _service;  
		private readonly IEventBus _eventBus;
		private readonly IPpeCheckNotifier _notifier;


		public TaskStepExecutionServiceTests()
		{
			_repository = Substitute.For<ITaskStepExecutionRepository>();
			_dateTimeProvider = Substitute.For<IDateTimeProvider>();
			_mapper = Substitute.For<IMapper>(); 
			_eventBus = Substitute.For<IEventBus>();
            _notifier = Substitute.For<IPpeCheckNotifier>();

			_service = new TaskStepExecutionService(
				_repository,
				_dateTimeProvider,
				_mapper, 
				_eventBus,
				_notifier
			);
		}

		// =========================
		// COMPLETE STEP SUCCESS
		// =========================
		[Fact]
        public async Task CompleteStepAsync_ShouldCompleteStep_WhenValidRequest()
        {
            var stepId = Guid.NewGuid();
            var workerId = Guid.NewGuid();
            var taskAssignmentId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            _dateTimeProvider.UtcNow.Returns(now);

            var assignment = new TaskAssignment
            {
                Id = taskAssignmentId,
                AssigneeId = workerId,
                IsAdhocTask = false
            };

            var step = new TaskStepExecution
            {
                Id = stepId,
                TaskAssignmentId = taskAssignmentId,
                TaskAssignment = assignment,
                Status = TaskStepExecutionStatus.InProgress,
                StepOrder = 1,
                CompletedAt = default
            };

            _repository.GetByIdDetail(stepId, default)
                .Returns(step);

            var nextStep = new TaskStepExecution
            {
                Id = Guid.NewGuid(),
                Status = TaskStepExecutionStatus.NotStarted,
                StepOrder = 2
            };

            _repository.GetNextStepAsync(taskAssignmentId, 1, default)
                .Returns(nextStep);

            var resultData = JsonSerializer.Serialize(new { test = "data" });
            var dto = new SubmitStepExecutionDto();
            dto.GetType().GetProperty("WorkerId")?.SetValue(dto, workerId);
            dto.GetType().GetProperty("ResultData")?.SetValue(dto, JsonDocument.Parse(resultData).RootElement);

            var resultDto = new TaskStepExecutionDto { Id = stepId, NextStepId = nextStep.Id };
            _mapper.Map<TaskStepExecutionDto>(step).Returns(resultDto);

            var result = await _service.CompleteStepAsync(stepId, dto);

            Assert.NotNull(result);
            Assert.Equal(stepId, result.Id);
            Assert.Equal(TaskStepExecutionStatus.Completed, step.Status);
            Assert.NotEqual(default, step.CompletedAt);
            Assert.Equal(TaskStepExecutionStatus.InProgress, nextStep.Status);
            await _repository.Received(1).SaveChangesAsync(default);
        }

        // =========================
        // COMPLETE STEP SUCCESS - NO NEXT STEP
        // =========================
        [Fact]
        public async Task CompleteStepAsync_ShouldCompleteLastStep_WhenNoNextStep()
        {
            var stepId = Guid.NewGuid();
            var workerId = Guid.NewGuid();
            var taskAssignmentId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            _dateTimeProvider.UtcNow.Returns(now);

            var assignment = new TaskAssignment
            {
                Id = taskAssignmentId,
                AssigneeId = workerId,
                IsAdhocTask = false
            };

            var step = new TaskStepExecution
            {
                Id = stepId,
                TaskAssignmentId = taskAssignmentId,
                TaskAssignment = assignment,
                Status = TaskStepExecutionStatus.InProgress,
                StepOrder = 3,
                CompletedAt = default
            };

            _repository.GetByIdDetail(stepId, default)
                .Returns(step);

            _repository.GetNextStepAsync(taskAssignmentId, 3, default)
                .Returns((TaskStepExecution)null);

            var resultData = JsonSerializer.Serialize(new { test = "data" });
            var dto = new SubmitStepExecutionDto();
            dto.GetType().GetProperty("WorkerId")?.SetValue(dto, workerId);
            dto.GetType().GetProperty("ResultData")?.SetValue(dto, JsonDocument.Parse(resultData).RootElement);

            var resultDto = new TaskStepExecutionDto { Id = stepId, NextStepId = null };
            _mapper.Map<TaskStepExecutionDto>(step).Returns(resultDto);

            var result = await _service.CompleteStepAsync(stepId, dto);

            Assert.NotNull(result);
            Assert.Null(result.NextStepId);
            await _repository.Received(1).SaveChangesAsync(default);
        }

        // =========================
        // COMPLETE STEP FAIL - NOT FOUND
        // =========================
        [Fact]
        public async Task CompleteStepAsync_ShouldThrow_WhenStepNotFound()
        {
            var stepId = Guid.NewGuid();

            _repository.GetByIdDetail(stepId, default)
                .Returns((TaskStepExecution)null);

            var dto = new SubmitStepExecutionDto();

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.CompleteStepAsync(stepId, dto));
        }

        // =========================
        // COMPLETE STEP FAIL - WRONG WORKER
        // =========================
        [Fact]
        public async Task CompleteStepAsync_ShouldThrow_WhenWrongWorker()
        {
            var stepId = Guid.NewGuid();
            var workerId = Guid.NewGuid();
            var assignedWorkerId = Guid.NewGuid();

            var assignment = new TaskAssignment
            {
                Id = Guid.NewGuid(),
                AssigneeId = assignedWorkerId,
                IsAdhocTask = false
            };

            var step = new TaskStepExecution
            {
                Id = stepId,
                TaskAssignment = assignment,
                Status = TaskStepExecutionStatus.InProgress
            };

            _repository.GetByIdDetail(stepId, default)
                .Returns(step);

            var dto = new SubmitStepExecutionDto();
            dto.GetType().GetProperty("WorkerId")?.SetValue(dto, workerId);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CompleteStepAsync(stepId, dto));
        }

        // =========================
        // COMPLETE STEP FAIL - ADHOC TASK
        // =========================
        [Fact]
        public async Task CompleteStepAsync_ShouldThrow_WhenAdhocTask()
        {
            var stepId = Guid.NewGuid();
            var workerId = Guid.NewGuid();

            var assignment = new TaskAssignment
            {
                Id = Guid.NewGuid(),
                AssigneeId = workerId,
                IsAdhocTask = true
            };

            var step = new TaskStepExecution
            {
                Id = stepId,
                TaskAssignment = assignment,
                Status = TaskStepExecutionStatus.InProgress
            };

            _repository.GetByIdDetail(stepId, default)
                .Returns(step);

            var dto = new SubmitStepExecutionDto();
            dto.GetType().GetProperty("WorkerId")?.SetValue(dto, workerId);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CompleteStepAsync(stepId, dto));
        }

        // =========================
        // COMPLETE STEP FAIL - NOT IN PROGRESS
        // =========================
        [Fact]
        public async Task CompleteStepAsync_ShouldThrow_WhenNotInProgress()
        {
            var stepId = Guid.NewGuid();
            var workerId = Guid.NewGuid();

            var assignment = new TaskAssignment
            {
                Id = Guid.NewGuid(),
                AssigneeId = workerId,
                IsAdhocTask = false
            };

            var step = new TaskStepExecution
            {
                Id = stepId,
                TaskAssignment = assignment,
                Status = TaskStepExecutionStatus.Completed
            };

            _repository.GetByIdDetail(stepId, default)
                .Returns(step);

            var dto = new SubmitStepExecutionDto();
            dto.GetType().GetProperty("WorkerId")?.SetValue(dto, workerId);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CompleteStepAsync(stepId, dto));
        }

        // =========================
        // COMPLETE STEP FAIL - ALREADY COMPLETED
        // =========================
        [Fact]
        public async Task CompleteStepAsync_ShouldThrow_WhenAlreadyCompleted()
        {
            var stepId = Guid.NewGuid();
            var workerId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var assignment = new TaskAssignment
            {
                Id = Guid.NewGuid(),
                AssigneeId = workerId,
                IsAdhocTask = false
            };

            var step = new TaskStepExecution
            {
                Id = stepId,
                TaskAssignment = assignment,
                Status = TaskStepExecutionStatus.InProgress,
                CompletedAt = now.AddHours(-1)
            };

            _repository.GetByIdDetail(stepId, default)
                .Returns(step);

            var dto = new SubmitStepExecutionDto();
            dto.GetType().GetProperty("WorkerId")?.SetValue(dto, workerId);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CompleteStepAsync(stepId, dto));
        }

        // =========================
        // GET STEP DETAIL SUCCESS
        // =========================
        [Fact]
        public async Task GetStepDetailAsync_ShouldReturnDetail()
        {
            var stepId = Guid.NewGuid();
            var step = new TaskStepExecution
            {
                Id = stepId,
                Status = TaskStepExecutionStatus.InProgress,
                StepOrder = 1
            };

            _repository.GetByIdAsync(stepId, default)
                .Returns(step);

            var detailDto = new TaskStepExecutionDetailDto
            {
                Id = stepId,
                Status = TaskStepExecutionStatus.InProgress
            };

            _mapper.Map<TaskStepExecutionDetailDto>(step).Returns(detailDto);

            var result = await _service.GetStepDetailAsync(stepId);

            Assert.NotNull(result);
            Assert.Equal(stepId, result.Id);
        }

        // =========================
        // GET STEP DETAIL FAIL - NOT FOUND
        // =========================
        [Fact]
        public async Task GetStepDetailAsync_ShouldThrow_WhenNotFound()
        {
            var stepId = Guid.NewGuid();

            _repository.GetByIdAsync(stepId, default)
                .Returns((TaskStepExecution)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.GetStepDetailAsync(stepId));
        }
    }
}