namespace CleanOpsAi.Modules.Scoring.Domain.Enums
{
	public enum ScoringRetrainRunStatus
	{
		Queued = 0,
		Running = 1,
		Failed = 2,
		Succeeded = 3,
		Promoted = 4,
		Rejected = 5,
	}
}
