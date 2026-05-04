namespace CleanOpsAi.Modules.Scoring.Infrastructure.Options
{
	public class ScoringServiceOptions
	{
		public string BaseUrl { get; set; } = "http://localhost:8000";
		public string EvaluateBatchPath { get; set; } = "/evaluate-batch";
		public string EvaluateUrlVisualizeLinkPath { get; set; } = "/evaluate-url-visualize-link";
		public string PpeEvaluatePath { get; set; } = "/ppe/evaluate";
		public int TimeoutSeconds { get; set; } = 120;
	}
}
