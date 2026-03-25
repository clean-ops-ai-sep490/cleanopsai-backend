using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.QualityControl.Domain.Entities;

namespace CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories
{
	public interface INotificationRecipientRepository : IBaseRepo<NotificationRecipient, Guid>
	{
		Task<(PaginatedResult<NotificationRecipient> Page, int UnreadCount)>GetPagedByRecipientAsync(
				Guid recipientId,
				PaginationRequest paginationRequest,
				bool? isRead,
				CancellationToken ct = default);

		Task<NotificationRecipient?> GetDetailAsync(Guid notificationId,Guid recipientId, CancellationToken ct = default);

		Task<bool> MarkAsReadAsync(Guid notificationId, Guid recipientId, CancellationToken ct = default);

		Task<int> MarkAllAsReadAsync(Guid recipientId, CancellationToken ct = default);
	}
}
