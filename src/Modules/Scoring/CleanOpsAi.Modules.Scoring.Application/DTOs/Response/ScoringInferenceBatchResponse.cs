using System.Text.Json;
using System.Text.Json.Serialization;

namespace CleanOpsAi.Modules.Scoring.Application.DTOs.Response
{
	public class ScoringInferenceBatchResponse
	{
		[JsonPropertyName("env")]
		public string? EnvironmentKey { get; set; }

		[JsonPropertyName("env_label")]
		public string? EnvironmentLabel { get; set; }

		[JsonPropertyName("max_batch_images")]
		public int? MaxBatchImages { get; set; }

		[JsonPropertyName("pending_lower_bound")]
		public double? PendingLowerBound { get; set; }

		[JsonPropertyName("summary")]
		public ScoringInferenceSummary Summary { get; set; } = new();

		[JsonPropertyName("results")]
		public List<ScoringInferenceResult> Results { get; set; } = new();

		[JsonExtensionData]
		public Dictionary<string, JsonElement> AdditionalData { get; set; } = new();
	}

	public class ScoringInferenceSummary
	{
		[JsonPropertyName("total_requested")]
		public int TotalRequested { get; set; }

		[JsonPropertyName("processed")]
		public int Processed { get; set; }

		[JsonPropertyName("skipped")]
		public int Skipped { get; set; }

		[JsonPropertyName("pass")]
		public int Pass { get; set; }

		[JsonPropertyName("pending")]
		public int Pending { get; set; }

		[JsonPropertyName("fail")]
		public int Fail { get; set; }

		[JsonExtensionData]
		public Dictionary<string, JsonElement> AdditionalData { get; set; } = new();
	}

	public class ScoringInferenceResult
	{
		[JsonPropertyName("id")]
		public int? Id { get; set; }

		[JsonPropertyName("source_type")]
		public string? SourceType { get; set; }

		[JsonPropertyName("source")]
		public string? Source { get; set; }

		[JsonPropertyName("scoring")]
		public ScoringInferenceScore? Scoring { get; set; }

		[JsonExtensionData]
		public Dictionary<string, JsonElement> AdditionalData { get; set; } = new();
	}

	public class ScoringInferenceScore
	{
		[JsonPropertyName("verdict")]
		public string? Verdict { get; set; }

		[JsonPropertyName("quality_score")]
		public double QualityScore { get; set; }

		[JsonPropertyName("base_clean_score")]
		public double? BaseCleanScore { get; set; }

		[JsonPropertyName("object_penalty")]
		public double? ObjectPenalty { get; set; }

		[JsonPropertyName("pass_threshold")]
		public double? PassThreshold { get; set; }

		[JsonPropertyName("reasons")]
		public List<string>? Reasons { get; set; }

		[JsonExtensionData]
		public Dictionary<string, JsonElement> AdditionalData { get; set; } = new();
	}

	public class ScoringVisualizationLinkResponse
	{
		[JsonPropertyName("source_type")]
		public string? SourceType { get; set; }

		[JsonPropertyName("source")]
		public string? Source { get; set; }

		[JsonPropertyName("env")]
		public string? EnvironmentKey { get; set; }

		[JsonPropertyName("visualization")]
		public ScoringVisualizationMetadata? Visualization { get; set; }

		[JsonPropertyName("scoring")]
		public ScoringInferenceScore? Scoring { get; set; }

		[JsonExtensionData]
		public Dictionary<string, JsonElement> AdditionalData { get; set; } = new();
	}

	public class ScoringVisualizationMetadata
	{
		[JsonPropertyName("token")]
		public string? Token { get; set; }

		[JsonPropertyName("url")]
		public string? Url { get; set; }

		[JsonPropertyName("mime_type")]
		public string? MimeType { get; set; }

		[JsonPropertyName("byte_size")]
		public int? ByteSize { get; set; }

		[JsonPropertyName("ttl_seconds")]
		public int? TtlSeconds { get; set; }

		[JsonPropertyName("expires_at_utc")]
		public string? ExpiresAtUtc { get; set; }

		[JsonExtensionData]
		public Dictionary<string, JsonElement> AdditionalData { get; set; } = new();
	}
}
