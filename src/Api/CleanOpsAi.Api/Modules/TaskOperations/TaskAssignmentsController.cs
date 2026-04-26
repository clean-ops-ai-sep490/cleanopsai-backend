using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.TaskOperations
{
	[Route("api/[controller]")]
	[ApiController]
	public class TaskAssignmentsController : ControllerBase
	{
		private readonly ITaskAssignmentService _taskAssignmentService;

		public TaskAssignmentsController(ITaskAssignmentService taskAssignmentService)
		{
			_taskAssignmentService = taskAssignmentService;
		}

		[HttpGet]
		[SwaggerOperation(
			Summary = "Get task assignments with filtering & pagination",
			Description = "Retrieves a paginated list of task assignments with optional filters such as assignee, date range, and status.",
			Tags = new[] { "TaskAssignment" }
		)]
		[ProducesResponseType(typeof(PaginatedResult<TaskAssignmentDto>), StatusCodes.Status200OK)]
		public async Task<IActionResult> Gets([FromQuery] TaskAssignmentFilter filter,
		[FromQuery] PaginationRequest request, CancellationToken ct)
		{
			var result = await _taskAssignmentService.Gets(filter, request, ct);
			return Ok(result);
		}

		[HttpGet("{id:guid}")]
		[SwaggerOperation(
			Summary = "Get Task Assignment by ID",
			Description = "Retrieves a task assignment by its ID. Returns the TaskAssignmentDto if found, otherwise 404 Not Found.",
			Tags = new[] { "TaskAssignment" }
		)]
		[ProducesResponseType(typeof(TaskAssignmentDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
		{
			var taskAssignment = await _taskAssignmentService.GetById(id, ct);
			if (taskAssignment == null) return NotFound();
			return Ok(taskAssignment);
		}

		[HttpPost("{id:guid}/start")]
		[SwaggerOperation(
			Summary = "Start a task assignment",
			Description = "Starts a task assignment for a specific worker. The task must be in NotStarted status and assigned to the provided workerId.",
			Tags = new[] { "TaskAssignment" }
		)]
		[ProducesResponseType(typeof(StartTaskDto), StatusCodes.Status200OK)]
		public async Task<IActionResult> StartTask([FromRoute] Guid id, [FromBody] StartTaskRequest request,
		CancellationToken ct)
		{
			var result = await _taskAssignmentService
				.StartTaskAsync(id, request.WorkerId, ct);

			return Ok(result);
		}

		[HttpPost("{id}/complete")]
		[SwaggerOperation(
			Summary = "Complete a task assignment",
			Description = "Marks a task assignment as completed. For SOP-based tasks, all steps must be completed. Adhoc tasks can be completed directly.",
			Tags = new[] { "TaskAssignment" }
		)]
		[ProducesResponseType(typeof(StartTaskDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> CompleteTask(Guid id, TaskCompletedDto dto, CancellationToken ct)
		{
			var result = await _taskAssignmentService.CompleteTaskAsync(id, dto, ct);
			return Ok(result);
		}

		[Authorize(Roles = "Manager")]
		[HttpPut("{id:guid}")]
		[SwaggerOperation(
			Summary = "Update Task Assignment",
			Description = "Updates the properties of a task assignment. Only allowed when status is NotStarted and task is not adhoc.",
			Tags = new[] { "TaskAssignment" }
		)]
		[ProducesResponseType(typeof(TaskAssignmentDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Update(Guid id, [FromBody] TaskAssignmentUpdateDto dto, CancellationToken ct)
		{
			var updated = await _taskAssignmentService.Update(id, dto, ct);
			return Ok(updated);
		}

		[HttpPatch("{id:guid}/status")]
		[SwaggerOperation(
		Summary = "Update Task Assignment Status",
		Description = "Updates only the status of a task assignment. Accepts a TaskAssignmentStatus value. " +
					  "Automatically updates LastModified and LastModifiedBy fields.",
		Tags = new[] { "TaskAssignment" })]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] TaskAssignmentStatus status)
		{
			var result = await _taskAssignmentService.UpdateStatus(id, status);
			if (!result) return NotFound();
			return Ok();
		}

		[HttpDelete("{id:guid}")]
		[SwaggerOperation(
			Summary = "Delete Task Assignment",
			Description = "Soft deletes a task assignment by its ID. The record will not be physically removed, " +
					  "but marked as deleted (IsDeleted = true).",
			Tags = new[] { "TaskAssignment" }
		)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Delete(Guid id)
		{
			var result = await _taskAssignmentService.Delete(id);
			if (!result) return NotFound();
			return Ok();
		}

        [HttpPost("adhoc")]
        [SwaggerOperation(
			Summary = "Create Adhoc Task",
			Description = "Creates a manual (adhoc) task without schedule and SOP steps. " +
						  "This task is assigned directly to a worker and can be started immediately.",
			Tags = new[] { "TaskAssignment" }
		)]
        [ProducesResponseType(typeof(TaskAssignmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAdhocTask(
    [FromBody] CreateAdhocTaskDto request)
        {
            var result = await _taskAssignmentService.CreateAdhocTask(request);
            return Ok(result);
        }

        [HttpGet("workers-availability-by-area")]
        [SwaggerOperation(
            Summary = "Get Worker For Free in time",
            Description = "Get all worker and who have no task in time will be sort first",
            Tags = new[] { "TaskAssignment" }
        )]
        public async Task<IActionResult> GetWorkersAvailabilityByArea(
			Guid workAreaId,
			DateTime start,
			DateTime end,
			CancellationToken ct)
        {
            var result = await _taskAssignmentService
                .GetWorkersAvailableByAreaAsync(workAreaId, start, end, ct);

            return Ok(result);
        }
    }
}
