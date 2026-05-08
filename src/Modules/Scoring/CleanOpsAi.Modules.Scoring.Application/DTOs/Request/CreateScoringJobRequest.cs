namespace CleanOpsAi.Modules.Scoring.Application.DTOs.Request
{
	public class CreateScoringJobRequest
	{
		public string? RequestId { get; set; }

		public string EnvironmentKey { get; set; } = "LOBBY_CORRIDOR";

		public List<string> ImageUrls { get; set; } = new();

		public string? SubmittedByUserId { get; set; }
	}
}
