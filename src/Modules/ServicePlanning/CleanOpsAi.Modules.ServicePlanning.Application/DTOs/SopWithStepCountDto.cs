using CleanOpsAi.Modules.ServicePlanning.Domain.Entities; 

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs
{
	public class SopWithStepCountDto
	{
		public Sop Sop { get; set; } = null!;
		public int StepCount { get; set; }
	}
}
