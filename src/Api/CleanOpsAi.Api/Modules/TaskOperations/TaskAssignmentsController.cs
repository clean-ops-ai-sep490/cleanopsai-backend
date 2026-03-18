using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
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

		[HttpGet("{id:guid}")]
		[SwaggerOperation(
			Summary = "Get Task Assignment by ID",
			Description = "Retrieves a task assignment by its ID. Returns the TaskAssignmentDto if found, otherwise 404 Not Found.",
			Tags = new[] { "TaskAssignment" })]
		[ProducesResponseType(typeof(TaskAssignmentDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
		{
			var taskAssignment = await _taskAssignmentService.GetById(id, ct);
			if (taskAssignment == null) return NotFound();
			return Ok(taskAssignment);
		}

		[HttpPut("{id:guid}")]
		[SwaggerOperation(
		Summary = "Update Task Assignment",
		Description = "Updates the properties of a task assignment. Requires a TaskAssignmentDto payload. " +
					  "Automatically updates LastModified and LastModifiedBy fields.",
		Tags = new[] { "TaskAssignment" })]
		[ProducesResponseType(typeof(TaskAssignmentDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Update(Guid id, [FromBody] TaskAssignmentDto dto)
		{
			var updated = await _taskAssignmentService.Update(id, dto);
			if (updated == null) return NotFound();
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
		Tags = new[] { "TaskAssignment" })]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Delete(Guid id)
		{
			var result = await _taskAssignmentService.Delete(id);
			if (!result) return NotFound();
			return Ok();
		}
	}
}
