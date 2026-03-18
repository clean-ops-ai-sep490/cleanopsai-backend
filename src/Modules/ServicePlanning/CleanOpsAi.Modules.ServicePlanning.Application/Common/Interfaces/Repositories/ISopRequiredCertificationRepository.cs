namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories
{
	public interface ISopRequiredCertificationRepository
	{
		Task MergeAsync(
			Guid sopId,
			HashSet<Guid> certificationIds,
			CancellationToken cancellationToken = default);
	}
}
