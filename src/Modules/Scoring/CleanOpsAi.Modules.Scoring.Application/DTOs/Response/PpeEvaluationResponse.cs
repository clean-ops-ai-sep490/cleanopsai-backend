using System.Text.Json.Serialization;

namespace CleanOpsAi.Modules.Scoring.Application.DTOs.Response
{
	public class PpeEvaluationResponse
	{
		[JsonPropertyName("status")]
		public string? Status { get; set; }

		[JsonPropertyName("message")]
		public string? Message { get; set; }

		[JsonPropertyName("detected_items")]
		public List<PpeDetectedItemResponse> DetectedItems { get; set; } = new();

		[JsonPropertyName("missing_items")]
		public List<string> MissingItems { get; set; } = new();

		[JsonPropertyName("failed_images")]
		public List<PpeFailedImageResponse> FailedImages { get; set; } = new();
	}

	public class PpeDetectedItemResponse
	{
		[JsonPropertyName("name")]
		public string? Name { get; set; }

		[JsonPropertyName("confidence")]
		public double Confidence { get; set; }

		[JsonPropertyName("image_index")]
		public int ImageIndex { get; set; }
	}

	public class PpeFailedImageResponse
	{
		[JsonPropertyName("image_url")]
		public string? ImageUrl { get; set; }

		[JsonPropertyName("image_index")]
		public int ImageIndex { get; set; }

		[JsonPropertyName("error")]
		public string? Error { get; set; }
	}
}
