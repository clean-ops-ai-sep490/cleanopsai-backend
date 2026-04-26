namespace CleanOpsAi.Modules.Scoring.Application.DTOs.Request
{
	public class TriggerScoringRetrainRequest
	{
		public int LookbackDays { get; set; } = 7;

		public int MinReviewedSamples { get; set; } = 25;

		public int MaxSamplesPerBatch { get; set; } = 500;
	}
}
