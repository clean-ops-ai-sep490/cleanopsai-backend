using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ClientManagement.Domain.Entities
{
	[Table("contract_shifts")]
	public class ContractShift : BaseAuditableEntity
	{
		public Guid ContractId { get; set; }
		public Contract Contract { get; set; } = null!; 

		public Guid WorkAreaId { get; set; }
		public WorkArea WorkArea { get; set; } = null!;

		public TimeOnly ShiftStart { get; set; }
		public TimeOnly ShiftEnd { get; set; }

		public int BreakMinutes { get; set; }
		public int RequiredWorkers { get; set; }

		public DayType DayType { get; set; }
	}
}
