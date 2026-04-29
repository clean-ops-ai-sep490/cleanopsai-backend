namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events
{
	public class PpeCheckRequestedEvent
	{
		public Guid TaskStepExecutionId { get; set; }
		public IReadOnlyCollection<string> ImageUrls { get; set; } = [];
		public IReadOnlyCollection<string> RequiredObjects { get; set; } = [];
		public double MinConfidence { get; set; } = 0.25d;
	}

	public class PpeCheckCompletedEvent
	{
		public Guid TaskStepExecutionId { get; set; }
		public string Status { get; set; } = "ERROR";
		public string? Message { get; set; }
		public List<PpeDetectedItem> DetectedItems { get; set; } = [];
		public List<string> MissingItems { get; set; } = [];
		public List<PpeFailedImage> FailedImages { get; set; } = [];
		public List<string> ImageUrls { get; set; } = [];       
		public List<string> RequiredObjects { get; set; } = [];
	}
	 
	public class PpeDetectedItem
	{
		public string? Name { get; set; }
		public double Confidence { get; set; }
		public int ImageIndex { get; set; }
	}

	public class PpeFailedImage
	{
		public string? ImageUrl { get; set; }
		public int ImageIndex { get; set; }
		public string? Error { get; set; }
	}
}
