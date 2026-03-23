using CleanOpsAi.BuildingBlocks.Domain.Dtos; 

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events
{
	public record GenerateTaskAssignmentsRequestedEvent(
	Guid ScheduleId,
	Guid? AssigneeId, 
	Guid WorkAreaId,
	DateOnly FromDate,
	DateOnly ToDate,
	RecurrenceType RecurrenceType,      // enum gửi kèm
	RecurrenceConfig RecurrenceConfig,  // config detail gửi kèm
	int DurationMinutes,
	string Source                       // "manual" | "auto"
	);
}
