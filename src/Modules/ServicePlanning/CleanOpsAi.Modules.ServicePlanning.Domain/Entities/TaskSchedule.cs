using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ServicePlanning.Domain.Entities
{
	[Table("task_schedules")]
	public class TaskSchedule : BaseAuditableEntity
	{
		public Guid SopId { get; set; }

		public Guid SlaId { get; set; }

		public Guid? WorkAreaDetailId { get; set; }

		public string Name { get; set; } = null!;

		public string Description { get; set; } = null!;

		public string Metadata { get; set; } = null!;

		public RecurrenceType RecurrenceType { get; set; }

		//json
		public string RecurrenceConfig { get; set; } = null!;
	}
}


