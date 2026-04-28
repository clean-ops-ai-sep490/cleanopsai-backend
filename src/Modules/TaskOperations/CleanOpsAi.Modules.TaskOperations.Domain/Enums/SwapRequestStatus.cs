namespace CleanOpsAi.Modules.TaskOperations.Domain.Enums
{
	public enum SwapRequestStatus
	{
		PendingTargetApproval = 0,    // A vừa tạo, chờ B confirm
		PendingSupervisorApproval = 1,  
		Approved = 2,                
		RejectedByTarget = 3,         
		RejectedBySupervisor = 4,      
		Expired = 5,                  
		CancelledByRequester = 6       
	}
}
