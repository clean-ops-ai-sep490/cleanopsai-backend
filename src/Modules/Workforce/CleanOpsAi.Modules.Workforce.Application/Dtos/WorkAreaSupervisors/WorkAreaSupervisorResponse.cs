using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors
{
    public class WorkAreaSupervisorResponse
    {
        public Guid Id { get; set; }

        public Guid? WorkAreaId { get; set; }
        public Guid? WorkerId { get; set; }

        public Guid SupervisorId { get; set; }

        public string? WorkerName { get; set; }

        public DateTime Created { get; set; }
    }
}
