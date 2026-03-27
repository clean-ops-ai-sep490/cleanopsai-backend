namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
	public class GetSwapCandidatesDto
	{
		public Guid TaskAssignmentId { get; set; }
		public DateOnly? Date { get; set; }
		public TimeOnly? PreferredStartTime { get; set; }
	}
}
