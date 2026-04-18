using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Response; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.QualityControl
{
	[Route("api/[controller]")]
	[ApiController]
	public class NotificationRecipientsController : ControllerBase
	{
		private readonly INotificationRecipientService _service;

		public NotificationRecipientsController(INotificationRecipientService notificationRecipientService)
		{
			_service = notificationRecipientService;
		}

		[Authorize]
		[HttpGet]
		[SwaggerOperation(
			Summary = "Get Notifications by Recipient",
			Description = "Retrieves paginated notifications for the current user, with optional filtering by read status. Also returns total unread notification count.",
			Tags = new[] { "NotificationRecipient" }
		)]
		[ProducesResponseType(typeof(NotificationPagedResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetPagedByRecipient(
			[FromQuery] bool? isRead,
			[FromQuery] PaginationRequest request,
			CancellationToken ct = default)
		{
			var (page, unreadCount) = await _service.GetPagedByRecipientAsync(request, isRead, ct);

			var response = new NotificationPagedResponse(page, unreadCount);

			return Ok(response);
		}

		[Authorize]
		[HttpGet("{notificationId:guid}")]
		[SwaggerOperation(
			Summary = "Get Notification Detail",
			Description = "Retrieves detailed information of a specific notification for the current user. Returns null if the notification does not exist or does not belong to the user.",
			Tags = new[] { "NotificationRecipient" }
		)]
		[ProducesResponseType(typeof(NotificationDetailDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetDetail(Guid notificationId, CancellationToken ct = default)
		{
			if (notificationId == Guid.Empty)
			{
				return BadRequest("NotificationId cannot be empty.");
			}

			var result = await _service.GetDetailAsync(notificationId, ct);

			if (result is null)
			{
				return NotFound($"Notification with id {notificationId} not found.");
			}

			return Ok(result);
		}

		[Authorize]
		[HttpPatch("{notificationId:guid}/read")]
		[SwaggerOperation(
			Summary = "Mark Notification As Read",
			Description = "Marks a specific notification as read for the current user.",
			Tags = new[] { "NotificationRecipient" }
		)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken ct = default)
		{
			if (notificationId == Guid.Empty)
			{
				return BadRequest("NotificationId cannot be empty.");
			}

			var success = await _service.MarkAsReadAsync(notificationId, ct);

			if (!success)
			{
				return NotFound($"Notification with id {notificationId} not found or already marked as read.");
			}

			return NoContent();
		}

		[Authorize]
		[HttpPatch("read-all")]
		[SwaggerOperation(
			Summary = "Mark All Notifications As Read",
			Description = "Marks all notifications as read for the current user and returns the number of updated records.",
			Tags = new[] { "NotificationRecipient" }
		)]
		[ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
		public async Task<IActionResult> MarkAllAsRead(CancellationToken ct = default)
		{
			var updatedCount = await _service.MarkAllAsReadAsync(ct);

			return Ok(updatedCount);
		}
	}
}
