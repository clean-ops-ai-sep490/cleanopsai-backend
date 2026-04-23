namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
	public record PendingSupervisorCheckDto
	{
		public Guid ComplianceCheckId { get; init; }
		public Guid TaskStepExecutionId { get; init; }
		public double MinScore { get; init; }
		public int FailedImageCount { get; init; }
		public DateTime CreatedAt { get; init; }
		public List<CheckImageDto> Images { get; init; } = new();
	}

	public record CheckImageDto
	{
		public Guid ImageId { get; init; }
		public string ImageUrl { get; init; } = null!;
		public double? QualityScore { get; init; }
		public string? Verdict { get; init; }
	}
}
