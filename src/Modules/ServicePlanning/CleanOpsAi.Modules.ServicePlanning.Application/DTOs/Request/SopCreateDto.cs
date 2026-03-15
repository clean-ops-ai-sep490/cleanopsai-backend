using CleanOpsAi.Modules.ServicePlanning.Domain.Enums;

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request
{
	public class SopCreateDto
	{
		public string Name { get; set; } = null!;

		public string? Description { get; set; }

		public ServiceType ServiceType { get; set; }

		public EnvironmentType EnvironmentType { get; set; } 

		public List<SopStepCreateDto> Steps { get; set; } = new();

		public List<Guid>? RequiredSkillIds { get; set; }

		public List<Guid>? RequiredCertificationIds { get; set; }
	}
}
