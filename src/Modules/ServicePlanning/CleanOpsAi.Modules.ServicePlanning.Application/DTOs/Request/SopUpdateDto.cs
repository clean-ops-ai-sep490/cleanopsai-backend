using CleanOpsAi.Modules.ServicePlanning.Domain.Enums;

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request
{
	public class SopUpdateDto
	{  
		public string? Name { get; set; }
		public string? Description { get; set; }
		public ServiceType? ServiceType { get; set; }
		public Guid EnvironmentTypeId { get; set; }
		public List<SopStepUpdateDto>? Steps { get; set; }

		public List<Guid>? RequiredSkillIds { get; set; }

		public List<Guid>? RequiredCertificationIds { get; set; }
	}
}
