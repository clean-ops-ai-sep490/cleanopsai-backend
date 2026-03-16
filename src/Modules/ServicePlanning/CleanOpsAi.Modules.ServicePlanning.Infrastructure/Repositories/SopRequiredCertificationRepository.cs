using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Repositories
{
	public class SopRequiredCertificationRepository : ISopRequiredCertificationRepository
	{
		private readonly ServicePlanningDbContext _context;

		public SopRequiredCertificationRepository(ServicePlanningDbContext context)
		{
			_context = context;
		}

		public async Task MergeAsync(
			Guid sopId,
			HashSet<Guid> certificationIds,
			CancellationToken cancellationToken = default)
		{
			await _context.SopRequiredCertifications
				.Where(x => x.SopId == sopId && !certificationIds.Contains(x.CertificationId))
				.ExecuteDeleteAsync(cancellationToken);

			var existingIds = await _context.SopRequiredCertifications
				.Where(x => x.SopId == sopId)
				.Select(x => x.CertificationId)
				.ToListAsync(cancellationToken);

			var toAdd = certificationIds
				.Except(existingIds)
				.Select(id => new SopRequiredCertification
				{
					SopId = sopId,
					CertificationId = id
				});

			await _context.SopRequiredCertifications.AddRangeAsync(toAdd, cancellationToken);
		}
	}
}
