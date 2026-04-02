using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Request;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.QualityControl
{
	[Route("api/[controller]")]
	[ApiController]
	public class FcmTokensController : ControllerBase
	{
		private readonly IFcmTokenService _tokenService; 

		public FcmTokensController(IFcmTokenService fcmTokenService)
		{
			_tokenService = fcmTokenService; 
		}

		[Authorize]
		[HttpPost("register")]
		[SwaggerOperation(
			Summary = "Register FCM Token",
			Description = "Creates or updates an FCM token for the current user's device. If the device already has an active token, it will be updated. If the token is used by another device, that device will be deactivated. Supports both User and Worker",
			Tags = new[] { "FcmToken" }
		)]
		[ProducesResponseType(typeof(FcmTokenDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Register([FromBody] FcmTokenCreateDto dto, CancellationToken ct = default)
		{
			var result = await _tokenService.CreateOrUpdateAsync(dto, ct);
			return Ok(result);
		}

		[Authorize]
		[HttpPatch("deactivate")]
		[SwaggerOperation(
			Summary = "Deactivate FCM Token",
			Description = "Deactivates the FCM token for the specified device of the current user. Should be called on logout to prevent push notifications from being sent to logged-out devices.",
			Tags = new[] { "FcmToken" }
		)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> Deactivate([FromQuery] string uniqueId, CancellationToken ct = default)
		{
			await _tokenService.DeactivateTokenAsync(uniqueId, ct);
			return NoContent();
		}
	}
}
