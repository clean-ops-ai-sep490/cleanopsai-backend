namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs
{
	public class SopStepMetadataDto
	{
		public Guid Id { get; set; }
		public Guid SopId { get; set; }
		public Guid StepId { get; set; }
		public int StepOrder { get; set; }
		public string ConfigDetail { get; set; } = null!;
		public bool IsDeleted { get; set; }
	}
}
