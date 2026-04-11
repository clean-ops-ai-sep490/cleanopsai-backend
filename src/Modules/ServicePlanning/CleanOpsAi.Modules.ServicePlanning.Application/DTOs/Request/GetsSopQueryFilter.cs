using CleanOpsAi.Modules.ServicePlanning.Domain.Enums;

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request
{
	public class GetsSopQueryFilter
	{
		public string? Name { get; set; }

		public ServiceType? ServiceType { get; set; }

		public Guid? EnvironmentTypeId { get; set; }

		public List<Guid>? RequiredSkillIds { get; set; }

		public List<Guid>? RequiredCertificationIds { get; set; }

		public bool IsDescending { get; set; } = false;
	}
}
