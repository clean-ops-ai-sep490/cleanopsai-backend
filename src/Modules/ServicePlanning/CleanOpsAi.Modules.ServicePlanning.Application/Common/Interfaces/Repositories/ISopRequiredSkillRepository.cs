namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories
{
	public interface ISopRequiredSkillRepository
	{
		Task MergeAsync(Guid sopId, HashSet<Guid> skillIds);
	}
}
