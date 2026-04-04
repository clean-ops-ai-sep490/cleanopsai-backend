using CleanOpsAi.Modules.TaskOperations.Domain.Enums;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
	public class TaskAssignmentFilter
	{
		public Guid? AssigneeId { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public TaskAssignmentStatus? Status { get; set; } 
	}
}
