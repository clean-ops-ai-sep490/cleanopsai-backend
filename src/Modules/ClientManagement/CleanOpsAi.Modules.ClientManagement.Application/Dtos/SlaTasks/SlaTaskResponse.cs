using CleanOpsAi.BuildingBlocks.Domain.Dtos;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.SlaTasks
{
    public class SlaTaskResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid SlaId { get; set; }

        public string SlaName { get; set; }

        public RecurrenceType RecurrenceType { get; set; }

        public RecurrenceConfigSlaTask RecurrenceConfig { get; set; }
    }
}
