namespace CleanOpsAi.Modules.Scoring.Application.DTOs.Response
{
	public class ScoringAnnotationCandidateListItemResponse
	{
		public Guid CandidateId { get; set; }
		public Guid ResultId { get; set; }
		public Guid JobId { get; set; }
		public string RequestId { get; set; } = null!;
		public string EnvironmentKey { get; set; } = null!;
		public string CandidateStatus { get; set; } = null!;
		public string ImageUrl { get; set; } = null!;
		public string? VisualizationBlobUrl { get; set; }
		public string OriginalVerdict { get; set; } = null!;
		public string ReviewedVerdict { get; set; } = null!;
		public string SourceType { get; set; } = null!;
		public Guid? AssignedToUserId { get; set; }
		public DateTime CreatedAtUtc { get; set; }
		public DateTime? SubmittedAtUtc { get; set; }
		public DateTime? ApprovedAtUtc { get; set; }
		public bool HasAnnotation { get; set; }
		public int? AnnotationVersion { get; set; }
	}

	public class ScoringAnnotationCandidateDetailResponse
	{
		public Guid CandidateId { get; set; }
		public Guid ResultId { get; set; }
		public Guid JobId { get; set; }
		public string RequestId { get; set; } = null!;
		public string EnvironmentKey { get; set; } = null!;
		public string CandidateStatus { get; set; } = null!;
		public string ImageUrl { get; set; } = null!;
		public string? VisualizationBlobUrl { get; set; }
		public string OriginalVerdict { get; set; } = null!;
		public string ReviewedVerdict { get; set; } = null!;
		public string SourceType { get; set; } = null!;
		public Guid? AssignedToUserId { get; set; }
		public DateTime CreatedAtUtc { get; set; }
		public DateTime? SubmittedAtUtc { get; set; }
		public DateTime? ApprovedAtUtc { get; set; }
		public string PayloadJson { get; set; } = "{}";
		public string PreAnnotationJson { get; set; } = "{\"labels\":[]}";
		public string? SnapshotBlobKey { get; set; }
		public string? MetadataBlobKey { get; set; }
		public ScoringAnnotationResponse? Annotation { get; set; }
	}

	public class ScoringAnnotationResponse
	{
		public Guid AnnotationId { get; set; }
		public string AnnotationFormat { get; set; } = null!;
		public string LabelsJson { get; set; } = "[]";
		public string? ReviewerNote { get; set; }
		public int Version { get; set; }
		public Guid? CreatedByUserId { get; set; }
		public Guid? ApprovedByUserId { get; set; }
		public DateTime LastModifiedUtc { get; set; }
	}
}
