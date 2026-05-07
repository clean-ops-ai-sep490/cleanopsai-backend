namespace CleanOpsAi.Modules.Scoring.Application.DTOs.Request
{
	public class TriggerScoringRetrainRequest
	{
		public int LookbackDays { get; set; } = 7;

		public int MinReviewedSamples { get; set; } = 25;

		public int MinApprovedAnnotations { get; set; } = 100;

		public int MaxSamplesPerBatch { get; set; } = 500;

		public bool UseLastBatchTime { get; set; }
	}
}
