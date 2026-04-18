using CleanOpsAi.BuildingBlocks.Domain.Dtos;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
	public interface IRecurrenceExpander
	{
		IReadOnlyList<DateTime> Expand(
		RecurrenceType type,
		RecurrenceConfig config,
		DateOnly from,
		DateOnly to);
	}
}
