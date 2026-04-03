using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;

namespace CleanOpsAi.Modules.TaskOperations.Application.Services
{
	public class TaskStepExecutionService : ITaskStepExecutionService
	{
		private readonly ITaskStepExecutionRepository _repository;
		private readonly IDateTimeProvider _dateTimeProvider;

		public TaskStepExecutionService(
			ITaskStepExecutionRepository taskStepExecutionRepository,
			IDateTimeProvider dateTimeProvider)
		{
			_repository = taskStepExecutionRepository;
			_dateTimeProvider = dateTimeProvider;
		}

		public async Task<TaskStepExecutionDto> CompleteStepAsync(Guid id, SubmitStepExecutionDto dto, CancellationToken ct = default)
		{
			var step = await _repository.GetByIdDetail(id, ct)
				?? throw new NotFoundException(nameof(TaskStepExecution), id);

			var assignment = step.TaskAssignment;

			if (assignment.AssigneeId != dto.WorkerId)
				throw new BadRequestException("Not your task");

			if (assignment.IsAdhocTask)
				throw new BadRequestException("Adhoc task does not support step execution");

			if (step.Status != TaskStepExecutionStatus.InProgress)
				throw new BadRequestException("Step is not in progress");

			if (step.CompletedAt != default)
				throw new BadRequestException("Step already completed");

			step.ResultData = dto.ResultData.GetRawText();
			step.Status = TaskStepExecutionStatus.Completed;
			step.CompletedAt = _dateTimeProvider.UtcNow;

			var nextStep = await _repository.GetNextStepAsync(
					step.TaskAssignmentId,
					step.StepOrder,
					ct);

			if (nextStep != null)
			{
				nextStep.Status = TaskStepExecutionStatus.InProgress;
				nextStep.StartedAt = _dateTimeProvider.UtcNow;
			}

			await _repository.SaveChangesAsync(ct);

			return new TaskStepExecutionDto
			{
				Id = step.Id,
				SopStepId = step.SopStepId,
				StepOrder = step.StepOrder,
				Status = step.Status.ToString(),
				NextStepId = nextStep?.Id
			};

		}
	}
}
