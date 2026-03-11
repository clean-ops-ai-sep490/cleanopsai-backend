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
	public class SopsController : ControllerBase
	{
		private readonly ISopService _sopService;

		public SopsController(ISopService sopService)
		{
			_sopService = sopService;
		}

		[HttpGet("{id:guid}")]
		[SwaggerOperation(
			Summary = "Get SOP by Id",
			Description = "Retrieves detailed information of a Standard Operating Procedure (SOP), including all ordered steps and their configuration details.",
			Tags = new[] { "SOP" }
		)]
		[ProducesResponseType(typeof(SopDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetById(Guid id)
		{
			var result = await _sopService.GetSopByIdAsync(id);

			if (result == null)
				return NotFound();

			return Ok(result);
		}

		[HttpPost]
		[SwaggerOperation(
			Summary = "Create a new SOP",
			Description = "Creates a new Standard Operating Procedure (SOP) with ordered steps. Each step must reference an existing Step definition and include configuration details that conform to the Step's config schema.",
			Tags = new[] { "SOP" }
		)]
		[ProducesResponseType(typeof(SopDto), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Create([FromBody] SopCreateDto dto)
		{
			var result = await _sopService.CreateSopAsync(dto);

			return CreatedAtAction(
				nameof(GetById),
				new { id = result.Id },
				result
			);
		}

		[HttpPut("{id:guid}")]
		[SwaggerOperation(
			Summary = "Update a SOP",
			Description = "Updates an existing SOP. If Steps are provided, they will fully replace the existing steps. Each step must reference an existing Step definition and include valid configuration details.",
			Tags = new[] { "SOP" }
		)]
		[ProducesResponseType(typeof(SopDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Update(Guid id, [FromBody] SopUpdateDto dto)
		{
			var result = await _sopService.UpdateSopAsync(id, dto);
			if (result == null) return NotFound();
			return Ok(result);
		}

		[HttpDelete("{id:guid}")]
		[SwaggerOperation(
			Summary = "Delete a SOP",
			Description = "Soft Deletes a Standard Operating Procedure (SOP) and all its associated steps.",
			Tags = new[] { "SOP" }
		)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Delete(Guid id)
		{
			var result = await _sopService.DeleteSopAsync(id);
			if (!result) return NotFound();
			return NoContent();
		}
	}
}
