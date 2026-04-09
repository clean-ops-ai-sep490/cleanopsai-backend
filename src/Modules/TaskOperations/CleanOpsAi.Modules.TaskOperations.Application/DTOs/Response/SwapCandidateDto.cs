namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
	public class SwapCandidateDto
	{
		public Guid WorkerId { get; set; }
		public string AssigneeName { get; set; } = null!;
		public SwapTaskInfoDto Task { get; set; } = null!;
	}
}
