using CleanOpsAi.Modules.ServicePlanning.Domain.Enums;
using System.Text.Json;

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request
{
	public class TaskScheduleUpdateDto
	{
		public Guid SopId { get; set; }

		public Guid SlaTaskId { get; set; }

		public Guid SlaShiftId { get; set; }

		public Guid? WorkAreaDetailId { get; set; }

		public string Name { get; set; } = null!;

		public string Description { get; set; } = null!;

		public Guid? AssigneeId { get; set; }

		public RecurrenceType RecurrenceType { get; set; }

		public JsonElement RecurrenceConfig { get; set; }
	}
}
