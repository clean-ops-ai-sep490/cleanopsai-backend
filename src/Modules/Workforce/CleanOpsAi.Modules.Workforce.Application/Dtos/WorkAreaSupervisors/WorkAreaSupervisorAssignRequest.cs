using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors
{
    public class WorkAreaSupervisorAssignRequest
    {
        public Guid WorkAreaId { get; set; }
        public string SupervisorId { get; set; } = null!;       
        public List<Guid> WorkerIds { get; set; } = new(); // list worker cần giám sát
    }
}
