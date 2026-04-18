using CleanOpsAi.BuildingBlocks.Domain.Dtos; 

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.SlaTasks
{
    public class SlaTaskUpdateRequest
    {
        public string? Name { get; set; }

		public RecurrenceType? RecurrenceType { get; set; }
		public RecurrenceConfigSlaTask? RecurrenceConfig { get; set; }
	}
}
