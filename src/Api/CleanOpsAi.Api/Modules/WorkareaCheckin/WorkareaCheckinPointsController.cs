using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

namespace CleanOpsAi.Api.Modules.WorkareaCheckin
{
	[Route("api/[controller]")]
	[ApiController]
	public class WorkareaCheckinPointsController : ControllerBase
	{
		private readonly IWorkareaCheckinPointService _service;
		private readonly IQrCodeService _qrService;

		public WorkareaCheckinPointsController(IWorkareaCheckinPointService service, IQrCodeService qrService)
		{
			_service = service;
			_qrService = qrService;
		}

		[HttpGet]
		[SwaggerOperation(
			Summary = "Get paginated check-in points",
			Description = "Retrieves a paginated list of workarea check-in points with optional filtering.",
			Tags = new[] { "WorkareaCheckinPoint" }
		)]
		[ProducesResponseType(typeof(PaginatedResult<WorkareaCheckinPointDto>), StatusCodes.Status200OK)]
		public async Task<IActionResult> Gets(
		[FromQuery] GetsCheckinPointQuery query,
		[FromQuery] PaginationRequest request,
		CancellationToken ct = default)
		{
			var result = await _service.Gets(query, request, ct);
			return Ok(result);
		}

		[HttpGet("workarea/{workareaId:guid}/qr")]
		[Produces("image/png")]
		[SwaggerOperation(
			Summary = "Generate QR code for Workarea",
			Description = "Generates a PNG QR code representing the specified Workarea. This QR code can be scanned by workers to perform check-in within that workarea.",
			Tags = new[] { "WorkareaCheckinPoint" }
		)]
		[ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetQrByWorkarea(Guid workareaId, CancellationToken ct = default)
		{
			var checkinPoint = await _service.GetFirstByWorkarea(workareaId, ct);
			if (checkinPoint == null)
				return NotFound($"No check-in point found for workarea {workareaId}"); 

			var qrBytes = _qrService.GeneratePngFromObject(new
			{
				id = checkinPoint.Id,
				code = checkinPoint.Code
			});

			return File(qrBytes, "image/png");
		}

		[Authorize]
		[HttpGet("{id:guid}")]
		[SwaggerOperation(
			Summary = "Get Workarea Check-in Point by Id",
			Description = "Retrieves detailed information of a workarea check-in point.",
			Tags = new[] { "WorkareaCheckinPoint" }
		)]
		[ProducesResponseType(typeof(WorkareaCheckinPointDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> GetById(Guid id)
		{
			var result = await _service.GetByIdAsync(id);
			return Ok(result);
		}

		[Authorize]
		[HttpPost]
		[SwaggerOperation(
			Summary = "Create Workarea Check-in Point",
			Description = "Creates a new check-in point within a workarea. Code can be auto-generated or manually provided.",
			Tags = new[] { "WorkareaCheckinPoint" }
		)]
		[ProducesResponseType(typeof(WorkareaCheckinPointDto), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)] 
		public async Task<IActionResult> Create(
		[FromBody] WorkareaCheckinPointCreateDto request,
		CancellationToken ct)
		{
			var result = await _service.Create(request, ct);

			return CreatedAtAction(
				nameof(GetById),
				new { id = result.Id },
				result
			);
		}

		[Authorize]
		[HttpPut("{id:guid}")]
		[SwaggerOperation(
			Summary = "Update Workarea Check-in Point",
			Description = "Updates information of a workarea check-in point.",
			Tags = new[] { "WorkareaCheckinPoint" }
		)]
		[ProducesResponseType(typeof(WorkareaCheckinPointDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Update(
		Guid id,
		[FromBody] WorkareaCheckinPointUpdateDto request,
		CancellationToken ct)
		{
			var result = await _service.Update(id, request, ct);
			return Ok(result);
		}

		[Authorize]
		[HttpDelete("{id:guid}")]
		[SwaggerOperation(
			Summary = "Delete Workarea Check-in Point",
			Description = "Soft deletes a workarea check-in point (sets it to inactive).",
			Tags = new[] { "WorkareaCheckinPoint" }
		)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
		{
			await _service.Delete(id, ct);
			return NoContent();
		}

		[Authorize]
		[HttpPatch("{id:guid}/activate")]
		[SwaggerOperation(
			Summary = "Activate Workarea Check-in Point",
			Description = "Activates a workarea check-in point.",
			Tags = new[] { "WorkareaCheckinPoint" }
		)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
		{
			var result = await _service.Activate(id, ct);

			if (!result.Succeeded)
				return BadRequest(result.Errors);

			return NoContent();
		}

		[Authorize]
		[HttpPatch("{id:guid}/deactivate")]
		[SwaggerOperation(
			Summary = "Deactivate Workarea Check-in Point",
			Description = "Deactivates a workarea check-in point.",
			Tags = new[] { "WorkareaCheckinPoint" }
		)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
		{
			var result = await _service.Deactivate(id, ct);

			if (!result.Succeeded)
				return BadRequest(result.Errors);

			return NoContent();
		}
	}
}
