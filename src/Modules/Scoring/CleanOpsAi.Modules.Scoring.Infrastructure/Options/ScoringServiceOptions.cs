namespace CleanOpsAi.Modules.Scoring.Infrastructure.Options
{
	public class ScoringServiceOptions
	{
		public string BaseUrl { get; set; } = "http://localhost:8000";
		public string EvaluateBatchPath { get; set; } = "/evaluate-batch";
		public int TimeoutSeconds { get; set; } = 120;
	}
}
