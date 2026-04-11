using System.Text.Json; 

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Response
{
	public class SopStepDto
	{
		public Guid Id { get; set; }
		public Guid SopId { get; set; }
		public Guid StepId { get; set; }
		public int StepOrder { get; set; }
		public JsonElement ConfigDetail { get; set; }

		public StepInSopStepDto Step { get; set; } = null!;
	}

	public class StepInSopStepDto
	{ 
		public string ActionKey { get; set; } = null!;

		public string Name { get; set; } = null!;

		public string? Description { get; set; }
	}

}
