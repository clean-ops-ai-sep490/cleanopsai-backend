using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request;
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.ServicePlanning
{
	[Route("api/[controller]")]
	[ApiController]
	public class TaskSchedulesController : ControllerBase
	{
		private readonly ITaskScheduleService _taskScheduleService;

		public TaskSchedulesController(ITaskScheduleService taskScheduleService)
		{
			_taskScheduleService = taskScheduleService; 
		}

		[Authorize]
		[HttpGet]
		[SwaggerOperation(
		Summary = "Get task schedules with pagination",
		Description = "Retrieves a paginated list of task schedules. Each task schedule represents a planned cleaning or service task associated with a specific SOP, including recurrence configuration, assigned worker, and execution metadata.",
		Tags = new[] { "TaskSchedule" }
	)]
		[ProducesResponseType(typeof(PaginatedResult<TaskScheduleDto>), StatusCodes.Status200OK)]
		public async Task<IActionResult> Gets(
		[FromQuery] PaginationRequest request,
		CancellationToken ct)
		{
			var result = await _taskScheduleService.Gets(request, ct);
			return Ok(result);
		}

		[Authorize]
		[HttpGet("{id:guid}")]
		[SwaggerOperation(
			Summary = "Get Task Schedule by Id",
			Description = "Retrieves detailed information of a Task Schedule, including its configuration, SOP reference, and execution metadata snapshot.",
			Tags = new[] { "TaskSchedule" }
		)]
		[ProducesResponseType(typeof(TaskScheduleDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetById(Guid id)
		{
			var result = await _taskScheduleService.GetById(id); 

			return Ok(result);
		}

		[Authorize]
		[HttpPost]
		[SwaggerOperation(
			Summary = "Create Task Schedule",
			Description = "Creates a new Task Schedule based on a selected SOP and scheduling configuration. The system will snapshot the SOP steps into the schedule metadata to ensure execution consistency even if the SOP changes later.",
			Tags = new[] { "TaskSchedule" }
		)]
		[ProducesResponseType(typeof(TaskScheduleDto), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Create([FromBody] TaskScheduleCreateDto createDto, CancellationToken ct = default)
		{
			var result = await _taskScheduleService.Create(createDto, ct);

			return CreatedAtAction(
				nameof(GetById),
				new { id = result.Id },
				result
			);
		}

		[Authorize]
		[HttpPut("{id:guid}")]
		[SwaggerOperation(
			Summary = "Update Task Schedule",
			Description = "Updates an existing task schedule with the provided configuration, including SLA settings, recurrence configuration, and related metadata.", 
			Tags = new[] { "TaskSchedule" })]
		[ProducesResponseType(typeof(TaskScheduleDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Update(Guid id, [FromBody] TaskScheduleUpdateDto dto, CancellationToken ct = default)
		{
			var result = await _taskScheduleService.Update(id, dto, ct);
			return Ok(result);
			 
		}


		[Authorize]
		[HttpDelete("{id:guid}")]
		[SwaggerOperation(
			Summary = "Delete Task Schedule",
			Description = "Soft deletes a task schedule by marking it as deleted. The record remains in the system but will no longer be returned in active queries.",
			Tags = new[] { "TaskSchedule" }
		)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Delete(Guid id)
		{
			var deleted = await _taskScheduleService.Delete(id);
			if (!deleted) return NotFound();
			return NoContent();
		}

		[Authorize]
		[HttpPost("taskassignments/generate")]
		[SwaggerOperation(
			Summary = "Generate Task Assignments",
			Description = "Generates task assignments for one or more task schedules within a specified date range. " +
				  "This queues the generation process and returns immediately. The generated assignments will be created based on the recurrence configuration of each schedule.",
			Tags = new[] { "TaskAssignment" })]
		[ProducesResponseType(StatusCodes.Status202Accepted)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Generate([FromBody] GenerateTaskAssignmentsRequest request,
		CancellationToken ct)
		{
			if (request.FromDate > request.ToDate)
				return BadRequest("FromDate must lower than ToDate.");

			if (request.TaskScheduleIds.Count == 0)
				return BadRequest("Need a least 1 id");

			await _taskScheduleService.GenerateTaskAssigmentsAsync(request, ct); 

			return Accepted(new { message = "Đang xử lý." });
		}
	}
}
