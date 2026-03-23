using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors
{
    public class WorkAreaSupervisorUpdateRequest
    {
        public Guid WorkAreaId { get; set; }
        public string SupervisorId { get; set; } = null!;
        public List<Guid> WorkerIds { get; set; } = new();
    }
}
