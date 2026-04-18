using CleanOpsAi.BuildingBlocks.Infrastructure.Events;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Events
{
	public interface ITaskScheduleEventService
	{
		Task RequestGenerateAssignments(
		GenerateTaskAssignmentsRequestedEvent request,
		CancellationToken ct);
	}
}
