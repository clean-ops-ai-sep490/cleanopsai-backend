using System.Text.Json.Serialization;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
	public class TaskStepExecutionPpeCheckResponse
	{
		[JsonPropertyName("type")]
		public string Type { get; set; } = "ai-ppe-check";

		[JsonPropertyName("taskStepExecutionId")]
		public Guid TaskStepExecutionId { get; set; }

		[JsonPropertyName("sopStepId")]
		public Guid SopStepId { get; set; }

		[JsonPropertyName("stepOrder")]
		public int StepOrder { get; set; }

		[JsonPropertyName("checkedAt")]
		public DateTime CheckedAt { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; } = "ERROR";

		[JsonPropertyName("message")]
		public string Message { get; set; } = string.Empty;

		[JsonPropertyName("requiredPPE")]
		public List<TaskStepExecutionPpeRequiredItemResponse> RequiredPpe { get; set; } = new();

		[JsonPropertyName("imageUrls")]
		public List<string> ImageUrls { get; set; } = new();

		[JsonPropertyName("detectedItems")]
		public List<TaskStepExecutionPpeDetectedItemResponse> DetectedItems { get; set; } = new();

		[JsonPropertyName("missingItems")]
		public List<TaskStepExecutionPpeRequiredItemResponse> MissingItems { get; set; } = new();

		[JsonPropertyName("failedImages")]
		public List<TaskStepExecutionPpeFailedImageResponse> FailedImages { get; set; } = new();
	}

	public class TaskStepExecutionPpeRequiredItemResponse
	{
		[JsonPropertyName("actionKey")]
		public string ActionKey { get; set; } = string.Empty;

		[JsonPropertyName("name")]
		public string Name { get; set; } = string.Empty;
	}

	public class TaskStepExecutionPpeDetectedItemResponse
	{
		[JsonPropertyName("actionKey")]
		public string ActionKey { get; set; } = string.Empty;

		[JsonPropertyName("name")]
		public string Name { get; set; } = string.Empty;

		[JsonPropertyName("confidence")]
		public double Confidence { get; set; }

		[JsonPropertyName("imageIndex")]
		public int ImageIndex { get; set; }
	}

	public class TaskStepExecutionPpeFailedImageResponse
	{
		[JsonPropertyName("imageUrl")]
		public string ImageUrl { get; set; } = string.Empty;

		[JsonPropertyName("imageIndex")]
		public int ImageIndex { get; set; }

		[JsonPropertyName("error")]
		public string Error { get; set; } = string.Empty;
	}
}
