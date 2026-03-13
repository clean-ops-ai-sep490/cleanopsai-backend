using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request;
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Response;
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

			if (result == null)
				return NotFound();

			return Ok(result);
		}

		[HttpPost]
		[SwaggerOperation(
			Summary = "Create Task Schedule",
			Description = "Creates a new Task Schedule based on a selected SOP and scheduling configuration. The system will snapshot the SOP steps into the schedule metadata to ensure execution consistency even if the SOP changes later.",
			Tags = new[] { "TaskSchedule" }
		)]
		[ProducesResponseType(typeof(TaskScheduleDto), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Create([FromBody] TaskScheduleCreateDto createDto)
		{
			var result = await _taskScheduleService.Create(createDto);

			return CreatedAtAction(
				nameof(GetById),
				new { id = result.Id },
				result
			);
		}

		[HttpPut("{id:guid}")]
		[SwaggerOperation(
			Summary = "Update Task Schedule",
			Description = "Updates an existing task schedule with the provided configuration, including SLA settings, recurrence configuration, and related metadata.", 
			Tags = new[] { "TaskSchedule" })]
		[ProducesResponseType(typeof(TaskScheduleDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Update(Guid id, [FromBody] TaskScheduleUpdateDto dto)
		{
			try
			{
				var result = await _taskScheduleService.Update(id, dto);
				return Ok(result);
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
		}

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
	}
}
