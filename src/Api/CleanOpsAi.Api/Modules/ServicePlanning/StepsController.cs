using CleanOpsAi.BuildingBlocks.Application.Pagination;
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
	public class StepsController : ControllerBase
	{
		private readonly IStepService _stepService;

		public StepsController(IStepService stepService)
		{
			_stepService = stepService;
		}

		[HttpGet]
		[SwaggerOperation(
			Summary = "Get steps with pagination",
			Description = "Retrieves a paginated list of steps. Each step defines a reusable action within a Standard Operating Procedure (SOP), including its action key, description, and configurable schema used for dynamic execution.",
			Tags = new[] { "Step" }
		)]
		[ProducesResponseType(typeof(PaginatedResult<StepDto>), StatusCodes.Status200OK)]
		public async Task<IActionResult> Gets(
		[FromQuery] PaginationRequest request,
		CancellationToken ct)
		{
			var result = await _stepService.Gets(request, ct);
			return Ok(result);
		}

		[HttpGet("{id:guid}")]
		[SwaggerOperation(
			Summary = "Get step by ID",
			Description = "Retrieve the detailed information of a specific step using its unique identifier.",
			Tags = new[] { "Step" }
		)]
		[ProducesResponseType(typeof(StepDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetById(Guid id)
		{
			var step = await _stepService.GetStepById(id);

			if (step == null)
				return NotFound();

			return Ok(step);
		}

		[HttpPost]
		[SwaggerOperation(
			Summary = "Create a new step",
			Description = "Creates a new step definition and returns the created resource.",
			Tags = new[] { "Step" }
		)]
		[ProducesResponseType(typeof(StepDto), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Create([FromBody] StepCreateDto dto)
		{
			var result = await _stepService.CreateNewStep(dto);

			return CreatedAtAction(
				nameof(GetById),
				new { id = result.Id },
				result
			);
		}

		[HttpPatch("{id}")]
		[SwaggerOperation(
			Summary = "Update a step",
			Description = "Updates an existing step definition.",
			Tags = new[] { "Step" }
		)]
		[ProducesResponseType(typeof(StepDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Update(Guid id, [FromBody] StepUpdateDto dto)
		{
			var result = await _stepService.UpdateStep(id, dto);

			return Ok(result);
		}

		[HttpDelete("{id}")]
		[SwaggerOperation(
			Summary = "Delete a step",
			Description = "Soft delete a step definition.",
			Tags = new[] { "Step" }
		)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Delete(Guid id)
		{
			var result = await _stepService.DeleteStep(id);
			if (!result)
				return NotFound();

			return NoContent();
		}

	}
}
