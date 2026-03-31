using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Domain.Entities
{
    [Table("workarea_supervisors")]
    public class WorkAreaSupervisor : BaseAuditableEntity
    {
        public Guid? WorkAreaId { get; set; }   // từ ClientManagement

        public Guid? WorkerId { get; set; }     // từ Workforce

        public Guid UserId { get; set; } // từ Identity

        // chỉ navigation nội bộ module
        public virtual Worker Worker { get; set; } = null!;
    }
}
