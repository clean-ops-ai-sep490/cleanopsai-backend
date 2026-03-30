namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
	public interface IWorkerCertificationSkillQueryService
	{
		Task<List<Guid>> GetQualifiedWorkersAsync(
		   List<Guid> requiredSkillIds,
		   List<Guid> requiredCertificationIds,
		   CancellationToken ct);

		Task<bool> IsWorkerQualifiedAsync(
			Guid workerId,
			List<Guid> requiredSkillIds,
			List<Guid> requiredCertificationIds,
			CancellationToken ct);
	}
}
