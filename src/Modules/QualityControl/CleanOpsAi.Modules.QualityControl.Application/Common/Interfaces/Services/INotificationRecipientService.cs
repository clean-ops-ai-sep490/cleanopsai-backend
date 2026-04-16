using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Request;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Response; 
namespace CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services
{
	public interface INotificationRecipientService
	{
		Task<(PaginatedResult<NotificationListItemDto> Page, int UnreadCount)> GetPagedByRecipientAsync(
				PaginationRequest paginationRequest,
				bool? isRead,
				CancellationToken ct = default);

		Task<NotificationDetailDto> GetDetailAsync(Guid notificationId, CancellationToken ct = default);

		Task<bool> MarkAsReadAsync(Guid notificationId, CancellationToken ct = default);

		Task<int> MarkAllAsReadAsync(CancellationToken ct = default);

		Task<NotificationRecipientDto?> Create(NotificationRecipientCreateDto request, CancellationToken ct = default);
	}
}
