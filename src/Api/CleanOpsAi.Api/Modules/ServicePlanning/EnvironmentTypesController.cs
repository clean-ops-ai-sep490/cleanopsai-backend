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
	public class EnvironmentTypesController : ControllerBase
	{
		private readonly IEnvironmentTypeService _environmentTypeService;

		public EnvironmentTypesController(IEnvironmentTypeService environmentTypeService)
		{
			_environmentTypeService = environmentTypeService;
		}

		[HttpGet]
		[SwaggerOperation(
			Summary = "Get environment types with pagination",
			Description = "Retrieves a paginated list of environment types. Supports pagination parameters such as page number and page size.",
			Tags = new[] { "Environment Type" }
		)]
		[ProducesResponseType(typeof(PaginatedResult<EnvironmentTypeDto>), StatusCodes.Status200OK)]
		public async Task<IActionResult> Gets([FromQuery] PaginationRequest request, CancellationToken ct)
		{
			var result = await _environmentTypeService.Gets(request, ct);
			return Ok(result);
		}


		[HttpGet("{id:guid}")]
		[SwaggerOperation(
			Summary = "Get environment type by Id",
			Description = "Retrieves detailed information of a specific environment type by its unique identifier.",
			Tags = new[] { "Environment Type" }
		)]
		[ProducesResponseType(typeof(EnvironmentTypeDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
		{
			var result = await _environmentTypeService.GetById(id, ct);

			return Ok(result);
		}

		[HttpPost]
		[SwaggerOperation(
			Summary = "Create a new environment type",
			Description = "Creates a new environment type with the provided details such as name, description, and configuration metadata.",
			Tags = new[] { "Environment Type" }
		)]
		[ProducesResponseType(typeof(EnvironmentTypeDto), StatusCodes.Status201Created)]
		public async Task<IActionResult> Create([FromBody] EnvironmentTypeCreateDto dto, CancellationToken ct)
		{
			var result = await _environmentTypeService.Create(dto, ct);

			return CreatedAtAction(
				nameof(GetById),
				new { id = result.Id },
				result
			);
		}

		[HttpPut("{id:guid}")]
		[SwaggerOperation(
			Summary = "Update an environment type",
			Description = "Updates an existing environment type. Only provided fields will be updated.",
			Tags = new[] { "Environment Type" }
		)]
		[ProducesResponseType(typeof(EnvironmentTypeDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Update(Guid id, [FromBody] EnvironmentTypeUpdateDto dto, CancellationToken ct)
		{
			var result = await _environmentTypeService.Update(id, dto, ct);

			return Ok(result);
		}

		[HttpDelete("{id:guid}")]
		[SwaggerOperation(
			Summary = "Delete an environment type",
			Description = "Performs a soft delete on an environment type by marking it as inactive.",
			Tags = new[] { "Environment Type" }
		)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
		{
			var isDeleted = await _environmentTypeService.Delete(id, ct); 
			return NoContent();
		}

	}
}
