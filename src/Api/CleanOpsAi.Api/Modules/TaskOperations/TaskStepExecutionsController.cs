using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.TaskOperations
{
	[Route("api/[controller]")]
	[ApiController]
	public class TaskStepExecutionsController : ControllerBase
	{
		private readonly ITaskStepExecutionService _service;
		public TaskStepExecutionsController(ITaskStepExecutionService taskStepExecutionService)
		{
			_service = taskStepExecutionService;
		}

		[HttpPost("{id}/complete")]
		[SwaggerOperation(
			Summary = "Complete a task step",
			Description = "Submits result data for a step, marks it as completed, and automatically moves to the next step if available.",
			Tags = new[] { "TaskStepExecution" }
		)]
		[ProducesResponseType(typeof(TaskStepExecutionDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> CompleteStep(
			Guid id,
			[FromBody] SubmitStepExecutionDto dto,
			CancellationToken ct)
		{ 

			var result = await _service.CompleteStepAsync(id, dto, ct);

			return Ok(result);
		}

		[HttpGet("{id:guid}")]
		[SwaggerOperation(
			Summary = "Get task step execution detail",
			Description = "Retrieves detailed information of a specific task step execution, including status, configuration snapshot, and result data.",
			Tags = new[] { "TaskStepExecution" }
		)]
		[ProducesResponseType(typeof(TaskStepExecutionDetailDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
		{
			var result = await _service.GetStepDetailAsync(id, ct);
			return Ok(result);
		}

		[HttpPost("{id}/ppe-check")]
		[SwaggerOperation(
			Summary = "Request AI PPE check",
			Description = "Queues an AI PPE check for the step. Result will be pushed via SignalR event `ppe-check-updated` on hub `/hubs/compliance`.",
			Tags = new[] { "TaskStepExecution" }
		)]
		[ProducesResponseType(typeof(TaskStepExecutionPpeCheckResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> RequestPpeCheck(Guid id, CancellationToken ct)
		{
			var result = await _service.RequestPpeCheckAsync(id, ct);
			return Ok(result);
		}
	}
}
