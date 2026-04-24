using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.QualityControl.Application.Common.Mappings;
using CleanOpsAi.Modules.QualityControl.Domain.Entities;
using CleanOpsAi.Modules.QualityControl.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.QualityControl.Infrastructure.Repositories
{
	public class NotificationRecipientRepository : BaseRepo<NotificationRecipient, Guid>, INotificationRecipientRepository
	{
		public NotificationRecipientRepository(QualityControlDbContext context) : base(context)
		{
			
		}

		public async Task<NotificationRecipient?> GetDetailAsync(
			Guid notificationId,
			Guid recipientId,
			RecipientTypeEnum recipientType,
			CancellationToken ct = default)
		{
			var query = _context.NotificationRecipients
				.Include(r => r.AppNotification)
				.AsNoTracking()
				.Where(r => r.NotificationId == notificationId);

			query = RecipientTypeMapper.IsRoleBased(recipientType)
				? query.Where(r => r.RecipientType == recipientType)
				: query.Where(r => r.RecipientId == recipientId);

			return await query.FirstOrDefaultAsync(ct);
		}

		public async Task<(PaginatedResult<NotificationRecipient> Page, int UnreadCount)> GetPagedByRecipientAsync(
		Guid recipientId,
		RecipientTypeEnum recipientType,
		PaginationRequest paginationRequest,
		bool? isRead,
		CancellationToken ct = default)
		{
			IQueryable<NotificationRecipient> query;
			int unreadCount;

			if (RecipientTypeMapper.IsRoleBased(recipientType))
			{
				unreadCount = await _context.NotificationRecipients
					.CountAsync(r => r.RecipientType == recipientType && !r.IsRead, ct);
				query = _context.NotificationRecipients
					.Include(r => r.AppNotification)
					.Where(r => r.RecipientType == recipientType);
			}
			else
			{
				unreadCount = await _context.NotificationRecipients
					.CountAsync(r => r.RecipientId == recipientId && !r.IsRead, ct);
				query = _context.NotificationRecipients
					.Include(r => r.AppNotification)
					.Where(r => r.RecipientId == recipientId);
			}

			if (isRead.HasValue)
				query = query.Where(r => r.IsRead == isRead.Value);

			var orderedQuery = query.OrderByDescending(r => r.AppNotification.Created);
			var page = await orderedQuery.ToPaginatedResultAsync(paginationRequest, ct);
			return (page, unreadCount);
		}

		public async Task<int> MarkAllAsReadAsync(Guid recipientId, CancellationToken ct = default)
		=> await _context.NotificationRecipients
			.Where(r => r.RecipientId == recipientId && !r.IsRead)
			.ExecuteUpdateAsync(s => s
				.SetProperty(r => r.IsRead, true)
				.SetProperty(r => r.IsReadAt, DateTime.UtcNow), ct);


		public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid recipientId, CancellationToken ct = default)
		{
			var recipient = await _context.NotificationRecipients
			.FirstOrDefaultAsync(
				r => r.NotificationId == notificationId
				  && r.RecipientId == recipientId, ct);

			if (recipient is null || recipient.IsRead) return false;

			recipient.IsRead = true;
			recipient.IsReadAt = DateTime.UtcNow;
			await _context.SaveChangesAsync(ct);
			return true;
		}
	}
}
