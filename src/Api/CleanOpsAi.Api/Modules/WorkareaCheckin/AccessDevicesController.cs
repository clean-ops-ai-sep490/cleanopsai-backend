using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.WorkareaCheckin
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccessDevicesController : ControllerBase
	{
		private readonly IAccessDeviceService _service;

		public AccessDevicesController(IAccessDeviceService accessDeviceService)
		{
			_service = accessDeviceService;
		}

		[HttpGet("by-identifier/{identifier}")]
		[SwaggerOperation(
		   Summary = "Get access device by Identifier",
		   Description = "Retrieves access device details using its hardware identifier (e.g., Bluetooth Local Name or MAC address).",
		   Tags = new[] { "Access Device" }
		)]
		[ProducesResponseType(typeof(AccessDeviceDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetByIdentifier(string identifier, CancellationToken ct)
		{
			var result = await _service.GetByIdentifierAsync(identifier, ct);
			if (result == null) return NotFound();
			return Ok(result);
		}


		[HttpGet("checkin-point/{checkinPointId:guid}")]
		[SwaggerOperation(
			Summary = "Get access devices by Check-in Point",
			Description = "Retrieves a paginated list of access devices assigned to a specific work area check-in point.",
			Tags = new[] { "Access Device" }
		)]
		[ProducesResponseType(typeof(PaginatedResult<AccessDeviceDto>), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetByCheckinPoint(
		Guid checkinPointId,
		[FromQuery] PaginationRequest request,
		CancellationToken ct)
		{
			var result = await _service.GetByCheckinPointAsync(checkinPointId, request, ct);
			return Ok(result);
		}

		[HttpGet("{id:guid}")]
		[SwaggerOperation(
		Summary = "Get access device by Id",
		Description = "Retrieves detailed information of a specific access device, including its configuration and associated check-in point details.",
		Tags = new[] { "Access Device" }
	)]
		[ProducesResponseType(typeof(AccessDeviceDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
		{
			var result = await _service.GetById(id, ct);
			return Ok(result);
		}

		[Authorize]
		[HttpPost]
		[SwaggerOperation(
			Summary = "Create a new access device",
			Description = "Registers a new access control device (QR Static or BLE Beacon). If the device type is BLE Beacon, a unique hardware Identifier is required.",
			Tags = new[] { "Access Device" }
		)]
		[ProducesResponseType(typeof(AccessDeviceDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Create([FromBody] AccessDeviceCreateDto request, CancellationToken ct)
		{
			var result = await _service.Create(request, ct);

			return CreatedAtAction(
				nameof(GetById),
				new { id = result.Id },
				result
			);
		}
	}
}
