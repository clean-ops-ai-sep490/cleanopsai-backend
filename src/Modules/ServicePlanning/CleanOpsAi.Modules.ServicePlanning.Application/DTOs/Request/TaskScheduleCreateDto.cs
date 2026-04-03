using CleanOpsAi.BuildingBlocks.Domain.Dtos; 

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request
{
	public class TaskScheduleCreateDto
	{
		public Guid SopId { get; set; }

		public Guid SlaTaskId { get; set; }

		public Guid SlaShiftId { get; set; }

		public Guid WorkAreaId { get; set; }

		public Guid? WorkAreaDetailId { get; set; }

		public string Name { get; set; } = null!;

		public string Description { get; set; } = null!;

		public Guid? AssigneeId { get; set; }

		public string? AssigneeName { get; set; }

		public string? DisplayLocation { get; set; }

		public int DurationMinutes { get; set; }

		public RecurrenceType RecurrenceType { get; set; }

		public RecurrenceConfig RecurrenceConfig { get; set; } = null!;

		public DateOnly ContractStartDate { get; set; }
		public DateOnly? ContractEndDate { get; set; }
	}
}
