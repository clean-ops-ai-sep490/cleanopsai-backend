using System.Text.Json.Serialization;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
	public class TaskStepExecutionPpeCheckJobResponse
	{
		[JsonPropertyName("jobId")]
		public Guid JobId { get; set; }

		[JsonPropertyName("taskStepExecutionId")]
		public Guid TaskStepExecutionId { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; } = "PENDING";

		[JsonPropertyName("message")]
		public string Message { get; set; } = string.Empty;

		[JsonPropertyName("updatedAt")]
		public DateTime UpdatedAt { get; set; }

		[JsonPropertyName("result")]
		public TaskStepExecutionPpeCheckResponse? Result { get; set; }
	}
}
