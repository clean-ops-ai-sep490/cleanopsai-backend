using System.Text.Json; 

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request
{
	public class SopStepCreateDto
	{
		public Guid StepId { get; set; }
		public int StepOrder { get; set; }
		public JsonElement ConfigDetail { get; set; }
	}
}
