namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs
{
	public class TaskStepExecutionDto
	{
		public Guid Id { get; set; }
		public Guid SopStepId { get; set; }
		public int StepOrder { get; set; }
		public string Status { get; set; } = null!;
	}
}
