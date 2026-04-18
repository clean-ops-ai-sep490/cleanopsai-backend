using CleanOpsAi.Modules.ServicePlanning.Domain.Enums; 

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Response
{
	public class SopListDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public string? Description { get; set; }
		public ServiceType ServiceType { get; set; }
		public Guid EnvironmentTypeId { get; set; }
		public int Version { get; set; }

		public List<Guid>? RequiredSkillIds { get; set; }
		public List<Guid>? RequiredCertificationIds { get; set; }

		public int? StepCount { get; set; }
		public EnvironmentDto? EnvironmentType { get; set; }

	}
	 

}
