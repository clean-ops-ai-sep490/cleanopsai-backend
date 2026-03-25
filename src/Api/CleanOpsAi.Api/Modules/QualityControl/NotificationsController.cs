using CleanOpsAi.BuildingBlocks.Application;
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
	public class NotificationsController : ControllerBase
	{
		private readonly INotificationService _service;
		private readonly IUserContext _userContext;


		public NotificationsController(INotificationService notificationService, IUserContext userContext)
		{
			_service = notificationService;
			_userContext = userContext;
		}

		[Authorize(Policy = "AdminOrManager")]
		[HttpGet("{id:guid}")]
		[SwaggerOperation(
			Summary = "Get Notification By Id",
			Description = "Retrieves detailed information of a specific notification by its unique identifier.",
			Tags = new[] { "Notification" }
		)]
		[ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
		{
			if (id == Guid.Empty)
				return BadRequest("Notification id cannot be empty.");
			

			var result = await _service.GetById(id, ct);

			if (result is null)
				return NotFound($"Notification with id {id} not found.");

			return Ok(result);
		}

		[Authorize(Policy = "AdminOrManager")]
		[HttpPost]
		[SwaggerOperation(
			Summary = "Create Notification",
			Description = "Creates a new notification in the system.",
			Tags = new[] { "Notification" }
		)]
		[ProducesResponseType(typeof(NotificationDto), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Create([FromBody] NotificationCreateDto dto, CancellationToken ct = default)
		{
			if (dto is null)
			{
				return BadRequest("Request body cannot be null.");
			}

			var result = await _service.Create(dto, _userContext.UserId, _userContext.Role, ct);

			return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
		}

		[Authorize(Policy = "AdminOrManager")]
		[HttpPut("{id:guid}")]
		[SwaggerOperation(
			Summary = "Update Notification",
			Description = "Updates an existing notification by its unique identifier.",
			Tags = new[] { "Notification" }
		)]
		[ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Update(Guid id, [FromBody] NotificationUpdateDto dto, CancellationToken ct = default)
		{
			if (id == Guid.Empty)
			{
				return BadRequest("Notification id cannot be empty.");
			}

			if (dto is null)
			{
				return BadRequest("Request body cannot be null.");
			}

			var result = await _service.Update(id, dto, _userContext.UserId, ct);

			if (result is null)
			{
				return NotFound($"Notification with id {id} not found.");
			}

			return Ok(result);
		}

		[Authorize(Policy = "AdminOrManager")]
		[HttpDelete("{id:guid}")]
		[SwaggerOperation(
			Summary = "Delete Notification",
			Description = "Deletes a notification by its unique identifier.",
			Tags = new[] { "Notification" }
		)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
		{
			if (id == Guid.Empty)
			{
				return BadRequest("Notification id cannot be empty.");
			}

			var success = await _service.Delete(id, ct);

			if (!success)
			{
				return NotFound($"Notification with id {id} not found.");
			}

			return NoContent();
		}

		 
	}
}
