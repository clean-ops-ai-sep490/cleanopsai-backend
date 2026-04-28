using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors
{
    public class WorkerGroupResponse
    {
        public Guid? WorkerId { get; set; }
        public string? WorkerName { get; set; }
        public List<Guid> WorkAreaIds { get; set; } = new();
    }
}
