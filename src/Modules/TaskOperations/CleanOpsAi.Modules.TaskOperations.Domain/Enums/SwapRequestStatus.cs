namespace CleanOpsAi.Modules.TaskOperations.Domain.Enums
{
	public enum SwapRequestStatus
	{
		PendingTargetApproval = 0,    // A vừa tạo, chờ B confirm
		PendingManagerApproval = 1,   // B đồng ý, chờ Manager duyệt
		Approved = 2,                 // Manager duyệt → swap thành công
		RejectedByTarget = 3,         // B từ chối
		RejectedByManager = 4,        // Manager từ chối
		Expired = 5,                  // Quá hạn B chưa confirm
		CancelledByRequester = 6      // A tự hủy
	}
}
