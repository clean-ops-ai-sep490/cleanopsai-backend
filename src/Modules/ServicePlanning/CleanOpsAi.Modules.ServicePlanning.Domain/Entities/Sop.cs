using CleanOpsAi.BuildingBlocks.Domain;  
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ServicePlanning.Domain.Entities
{
	[Table("sops")]
	public class Sop : BaseAuditableEntity
	{
		public string Name { get; set; } = null!;

		public string? Description { get; set; }

		public ServiceType ServiceType { get; set; }

		public Guid EnvironmentTypeId { get; set; }

		public int Version { get; set; }

		public virtual ICollection<SopStep> SopSteps { get; set; } = new List<SopStep>();

		public virtual ICollection<TaskSchedule> TaskSchedules { get; set; } = new List<TaskSchedule>();

		public virtual ICollection<SopRequiredSkill> SopRequiredSkills { get; set; }
	= new List<SopRequiredSkill>();

		public virtual ICollection<SopRequiredCertification> SopRequiredCertifications { get; set; }
			= new List<SopRequiredCertification>();

		public EnvironmentType EnvironmentType { get; set; } = null!;
	}
}
