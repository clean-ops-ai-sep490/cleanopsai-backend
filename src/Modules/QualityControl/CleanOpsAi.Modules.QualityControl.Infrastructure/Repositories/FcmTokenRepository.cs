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

		public async Task<List<FcmToken>> GetActiveTokensByUserIdsAsync(List<Guid> userIds, CancellationToken ct = default)
		{
			return await _context.FcmTokens
				.Where(x => userIds.Contains(x.UserId) && x.IsActive)
				.ToListAsync(ct);
		}
	}
}
