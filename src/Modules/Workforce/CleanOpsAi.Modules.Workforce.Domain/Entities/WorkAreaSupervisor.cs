using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.Workforce.Domain.Entities
{
    [Table("workarea_supervisors")]
    public class WorkAreaSupervisor : BaseAuditableEntity
    {
        public Guid? WorkAreaId { get; set; }   // từ ClientManagement

        public Guid? WorkerId { get; set; }     // từ Workforce

        public string UserId { get; set; } = null!; // Suppervisor id from userId

        // chỉ navigation nội bộ module
        public virtual Worker Worker { get; set; } = null!;
    }
}
