namespace CleanOpsAi.Modules.Scoring.Domain.Enums
{
	public enum ScoringRetrainBatchStatus
	{
		Queued = 0,
		Running = 1,
		Failed = 2,
		Promoted = 3,
		Rejected = 4,
	}
}
