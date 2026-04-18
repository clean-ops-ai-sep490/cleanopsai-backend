namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
	public class ReviewSwapRequestDto // Manager duyệt
	{
		public Guid TaskSwapRequestId { get; set; }
		public bool IsApproved { get; set; }
		public string? ReviewNote { get; set; }
	}
}
