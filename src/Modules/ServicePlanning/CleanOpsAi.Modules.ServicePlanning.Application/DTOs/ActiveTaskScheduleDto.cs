using CleanOpsAi.BuildingBlocks.Domain.Dtos;
using System.Text.Json;

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs
{
	public class ActiveTaskScheduleDto
	{
		public Guid Id { get; set; }

		public Guid? AssigneeId { get; set; }

		public Guid WorkAreaId { get; set; } 

		public RecurrenceType RecurrenceType { get; set; }

		public JsonElement RecurrenceConfig { get; set; }

		public DateOnly ContractStartDate { get; set; }
		public DateOnly? ContractEndDate { get; set; }

		public int DurationMinutes { get; set; }
	}
}
