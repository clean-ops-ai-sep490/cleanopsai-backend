using CleanOpsAi.BuildingBlocks.Domain.Dtos;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.SlaTasks
{
    public class SlaTaskCreateRequest
    {
        public string Name { get; set; } = null!;

		public Guid SlaId { get; set; }

        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.Daily;

		public RecurrenceConfigSlaTask RecurrenceConfig { get; set; }
    }
}
