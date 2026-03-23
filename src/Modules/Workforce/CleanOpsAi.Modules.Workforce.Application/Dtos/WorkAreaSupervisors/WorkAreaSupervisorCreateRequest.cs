using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors
{
    public class WorkAreaSupervisorCreateRequest
    {
        public Guid? WorkAreaId { get; set; }
        public Guid? WorkerId { get; set; }

        public string SupervisorId { get; set; } = null!;
    }
}
