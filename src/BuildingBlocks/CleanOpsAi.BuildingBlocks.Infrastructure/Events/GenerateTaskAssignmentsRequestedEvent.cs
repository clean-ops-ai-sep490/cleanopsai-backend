using CleanOpsAi.BuildingBlocks.Domain.Dtos; 

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events
{
	public record GenerateTaskAssignmentsRequestedEvent(
	Guid ScheduleId,
	Guid? AssigneeId, 
	DateOnly FromDate,
	DateOnly ToDate,
	RecurrenceType RecurrenceType,      // enum gửi kèm
	RecurrenceConfig RecurrenceConfig,  // config detail gửi kèm
	string Source                       // "manual" | "auto"
	);
}
