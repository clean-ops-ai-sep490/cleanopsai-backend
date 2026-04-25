using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.TaskOperations.Domain.Entities
{
	[Table("task_assignments")]
	public class TaskAssignment : BaseAuditableEntity
	{
		public Guid TaskScheduleId { get; set; }

		public Guid AssigneeId { get; set; }

		public Guid OriginalAssigneeId { get; set; }

		public string AssigneeName { get; set; } = null!;
		public string OriginalAssigneeName { get; set; } = null!;

		public Guid WorkAreaId { get; set; }

		public TaskAssignmentStatus Status { get; set; }

		public string? TaskName { get; set; }

		public DateTime ScheduledStartAt { get; set; }
		public DateTime ScheduledEndAt { get; set; }

		public bool IsAdhocTask { get; set; }

		public string? NameAdhocTask { get; set; }

		public string? DisplayLocation { get; set; }

		public virtual ICollection<TaskStepExecution> TaskStepExecutions { get; set; } = new List<TaskStepExecution>();

		public virtual ICollection<TaskSwapRequest> TaskSwapRequests { get; set; } = new List<TaskSwapRequest>();
		public virtual ICollection<TaskSwapRequest> TargetTaskSwapRequests { get; set; } = new List<TaskSwapRequest>();

		public virtual ICollection<IssueReport> IssueReports { get; set; } = new List<IssueReport>();

		public virtual ICollection<EmergencyLeaveRequest> EmergencyLeaveRequests { get; set; } = new List<EmergencyLeaveRequest>();

		public virtual ICollection<EquipmentRequest> EquipmentRequests { get; set; } = new List<EquipmentRequest>();

		public virtual ICollection<AdHocRequest> AdHocRequests { get; set; } = new List<AdHocRequest>(); 
	}
}
