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
			Summary = "Run AI PPE check for a task step",
			Description = "Runs AI PPE validation for the current in-progress ai-ppe-check step using its PPE images and requiredPPE configuration from the step snapshot.",
			Tags = new[] { "TaskStepExecution" }
		)]
		[ProducesResponseType(typeof(TaskStepExecutionPpeCheckResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> EvaluatePpe(Guid id, CancellationToken ct)
		{
			var result = await _service.EvaluatePpeAsync(id, ct);
			return Ok(result);
		}

		[HttpPost("{id}/ppe-check/jobs")]
		[SwaggerOperation(
			Summary = "Submit AI PPE check job",
			Description = "Queues AI PPE validation for asynchronous processing and returns a job handle for polling.",
			Tags = new[] { "TaskStepExecution" }
		)]
		[ProducesResponseType(typeof(TaskStepExecutionPpeCheckJobResponse), StatusCodes.Status202Accepted)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> SubmitPpeCheckJob(Guid id, CancellationToken ct)
		{
			var result = await _service.EnqueuePpeCheckAsync(id, ct);
			return AcceptedAtAction(nameof(GetPpeCheckJobStatus), new { id }, result);
		}

		[HttpGet("{id}/ppe-check/jobs")]
		[SwaggerOperation(
			Summary = "Get AI PPE check job status",
			Description = "Returns asynchronous PPE check job status and final result when available.",
			Tags = new[] { "TaskStepExecution" }
		)]
		[ProducesResponseType(typeof(TaskStepExecutionPpeCheckJobResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetPpeCheckJobStatus(Guid id, CancellationToken ct)
		{
			var result = await _service.GetPpeCheckJobStatusAsync(id, ct);
			return Ok(result);
		}
	}
}
