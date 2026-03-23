using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors
{
    public class WorkAreaSupervisorAssignResponse
    {
        public Guid WorkAreaId { get; set; }
        public string SupervisorId { get; set; } = null!;
        public int TotalAssigned { get; set; }
        public List<WorkAreaSupervisorResponse> Assignments { get; set; } = new();
    }
}
