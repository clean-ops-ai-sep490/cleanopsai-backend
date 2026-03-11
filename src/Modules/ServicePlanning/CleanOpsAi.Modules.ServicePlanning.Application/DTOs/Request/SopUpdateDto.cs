using CleanOpsAi.Modules.ServicePlanning.Domain.Enums;

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request
{
	public class SopUpdateDto
	{
		public string? Name { get; set; }
		public string? Description { get; set; }
		public ServiceType? ServiceType { get; set; }
		public EnvironmentType? EnvironmentType { get; set; }
		public bool? IsRequiredSkill { get; set; }
		public bool? IsRequiredCertification { get; set; }
		public List<SopStepUpdateDto>? Steps { get; set; }
	}
}
