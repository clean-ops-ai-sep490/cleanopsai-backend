using System.Text.Json;

namespace CleanOpsAi.Modules.Scoring.Application.DTOs.Request
{
	public class UpsertScoringAnnotationRequest
	{
		public string? AnnotationFormat { get; set; } = "bbox-region-v1";

		public JsonElement Labels { get; set; }

		public string? ReviewerNote { get; set; }

		public bool Submit { get; set; }
	}

	public class ApproveScoringAnnotationCandidateRequest
	{
		public string? Note { get; set; }
	}

	public class RejectScoringAnnotationCandidateRequest
	{
		public string? Reason { get; set; }
	}
}
