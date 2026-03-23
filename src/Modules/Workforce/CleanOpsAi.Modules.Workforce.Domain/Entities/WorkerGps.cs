using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.Workforce.Domain.Entities
{
	[Table("worker_gps")]
	public class WorkerGps : BaseAuditableEntity
	{
		public Guid WorkerId { get; set; }

		public double? Latitude { get; set; }

		public double? Longitude { get; set; } 

		public bool? IsConfirmed { get; set; }

        public virtual Worker Worker { get; set; } = null!;
	}
}
