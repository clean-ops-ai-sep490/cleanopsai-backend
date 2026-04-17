namespace CleanOpsAi.Modules.Scoring.Application.DTOs.Request
{
	public class ReviewScoringResultRequest
	{
		public string Verdict { get; set; } = null!;
		public string? Reason { get; set; }
	}
}
