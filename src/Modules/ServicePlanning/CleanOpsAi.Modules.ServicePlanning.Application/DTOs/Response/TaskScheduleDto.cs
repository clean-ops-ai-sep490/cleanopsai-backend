using CleanOpsAi.BuildingBlocks.Domain.Dtos;
using System.Text.Json;

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Response
{
	public class TaskScheduleDto
	{
		public Guid Id { get; set; }

		public Guid SopId { get; set; }

		public Guid SlaTaskId { get; set; }

		public Guid SlaShiftId { get; set; }

		public Guid? WorkAreaDetailId { get; set; }

		public string Name { get; set; } = null!;

		public string Description { get; set; } = null!;

		public Guid? AssigneeId { get; set; }

		public JsonElement Metadata { get; set; }

		public RecurrenceType RecurrenceType { get; set; }

		public JsonElement RecurrenceConfig { get; set; }

		public bool IsActive { get; set; }  
	}
}
