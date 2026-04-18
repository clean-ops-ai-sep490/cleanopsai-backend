using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.QualityControl.Domain.Entities;

namespace CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories
{
	public interface IFcmTokenRepository : IBaseRepo<FcmToken, Guid>
	{
		Task<FcmToken?> GetActiveTokenAsync(string uniqueId, Guid userId, CancellationToken cancellationToken = default);

		Task<FcmToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

		Task<List<FcmToken>> GetActiveTokensByUserIdsAsync(List<Guid> userIds, CancellationToken ct = default);

		Task<List<FcmToken>> GetActiveTokensByUserAndWorkerIdsAsync(
			List<Guid> userIds,
			List<Guid> workerIds,
			CancellationToken ct = default);

		Task<List<FcmToken>> GetActiveTokensForPushAsync(
			List<NotificationRecipientEvent> recipients,
			CancellationToken cancellationToken = default);

		Task<FcmToken?> GetByUniqueIdAsync(string uniqueId, CancellationToken cancellationToken = default);
	}
}
