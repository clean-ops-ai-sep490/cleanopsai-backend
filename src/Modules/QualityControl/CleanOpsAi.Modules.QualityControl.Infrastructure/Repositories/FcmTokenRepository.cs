using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.QualityControl.Domain.Entities;
using CleanOpsAi.Modules.QualityControl.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.QualityControl.Infrastructure.Repositories
{
	public class FcmTokenRepository : BaseRepo<FcmToken, Guid>, IFcmTokenRepository
	{
		public FcmTokenRepository(QualityControlDbContext context) : base(context)
		{
			
		}

		public async Task<FcmToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
		{
			return await _context.FcmTokens
			.FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
		}

		public async Task<FcmToken?> GetActiveTokenAsync(string uniqueId, Guid userId, CancellationToken cancellationToken = default)
		{
			return await _context.FcmTokens
			.FirstOrDefaultAsync(x => x.UniqueId == uniqueId && x.UserId == userId && x.IsActive, cancellationToken);
		}

		public async Task<FcmToken?> GetByUniqueIdAsync(string uniqueId, CancellationToken cancellationToken = default)
		{
			return await _context.FcmTokens
				.FirstOrDefaultAsync(x => x.UniqueId == uniqueId, cancellationToken);
		}

		public async Task<List<FcmToken>> GetActiveTokensByUserIdsAsync(List<Guid> userIds, CancellationToken ct = default)
		{
			return await _context.FcmTokens
				.Where(x => userIds.Contains(x.UserId) && x.IsActive)
				.ToListAsync(ct);
		}

		public async Task<List<FcmToken>> GetActiveTokensByUserAndWorkerIdsAsync(
			List<Guid> userIds,
			List<Guid> workerIds,
			CancellationToken ct = default)
		{
			return await _context.FcmTokens
				.Where(t => t.IsActive)
				.Where(t => (userIds.Contains(t.UserId) && t.WorkerId == null) ||
							(workerIds.Contains(t.WorkerId!.Value)))
				.ToListAsync(ct);
		}

		public async Task<List<FcmToken>> GetActiveTokensForPushAsync(
			List<NotificationRecipientEvent> recipients,
			CancellationToken ct = default)
		{
			if (recipients == null || !recipients.Any())
				return new List<FcmToken>();

			var workerIds = recipients
				.Where(r => r.RecipientType == RecipientTypeEnum.Worker)
				.Select(r => r.RecipientId)
				.Distinct()
				.ToList();

			var supervisorUserIds = recipients
				.Where(r => r.RecipientType == RecipientTypeEnum.Supervisor)
				.Select(r => r.RecipientId)
				.Distinct()
				.ToList();

			return await _context.Set<FcmToken>()
				.Where(t => t.IsActive)
				.Where(t =>
					(workerIds.Count > 0 && t.WorkerId.HasValue && workerIds.Contains(t.WorkerId.Value)) ||
					(supervisorUserIds.Count > 0 && supervisorUserIds.Contains(t.UserId))
				)
				.ToListAsync(ct);
		}
	}
}
