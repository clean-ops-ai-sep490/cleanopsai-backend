using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
	public interface ISopRequirementsQueryService
	{
		Task<SopRequirementsIntegrated>	GetSopRequirementsByScheduleId(Guid taskScheduleId, CancellationToken ct);
	}
}
