using CleanOpsAi.Modules.ServicePlanning.Domain.Enums;

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Response
{
	public class SopDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public string? Description { get; set; }
		public ServiceType ServiceType { get; set; }
		public EnvironmentType EnvironmentType { get; set; }
		public bool IsRequiredSkill { get; set; }
		public bool IsRequiredCertification { get; set; }
		public int Version { get; set; }
		public List<SopStepDto> SopSteps { get; set; } = new();

		public List<Guid>? RequiredSkillIds { get; set; } 
		public List<Guid>? RequiredCertificationIds { get; set; }
	}
}
