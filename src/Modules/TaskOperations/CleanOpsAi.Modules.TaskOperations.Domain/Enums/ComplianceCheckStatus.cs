namespace CleanOpsAi.Modules.TaskOperations.Domain.Enums
{
	public enum ComplianceCheckStatus
	{
		Pending = 0,        // was created but not yet sent to AI (e.g. waiting for images or for a retry delay)
		Processing = 1,     // was sent to AI and is waiting for results
		Passed = 2,         
		Failed = 3,         
		PendingSupervisor = 4  // need super review 
	}
}
