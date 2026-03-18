namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request
{
	public class GenerateTaskAssignmentsRequest
	{
		public List<Guid> TaskScheduleIds { get; set; } = [];

		public DateOnly FromDate { get; set; }
		public DateOnly ToDate { get; set; }
	}
}
