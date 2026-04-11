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
	public class CheckinRecordsController : ControllerBase
	{
		private readonly ICheckinRecordService _checkinRecordService;

		public CheckinRecordsController(ICheckinRecordService checkinRecordService)
		{
			_checkinRecordService = checkinRecordService;
		}

		[Authorize]
		[HttpPost("checkin")]
		[SwaggerOperation(
			Summary = "Perform worker check-in",
			Description = "Records a check-in event using either QR code (WorkareaId) or BLE (DeviceUuid). Returns the record details including the unique ID needed for subsequent task steps.",
			Tags = new[] { "CheckinRecord" }
		)]
		[ProducesResponseType(typeof(CheckinResponseDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<CheckinResponseDto>> Checkin(
		[FromBody] CheckinRequestDto request,
		CancellationToken ct)
		{
			var result = await _checkinRecordService.Checkin(request, ct);

			return Ok(result);
		}
	}
}
