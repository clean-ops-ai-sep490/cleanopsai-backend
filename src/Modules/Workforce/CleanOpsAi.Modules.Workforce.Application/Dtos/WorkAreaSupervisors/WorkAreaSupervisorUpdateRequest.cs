using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors
{
    public class WorkAreaSupervisorUpdateRequest
    {
        public Guid? WorkerId { get; set; }
        public string? UserId { get; set; }
    }
}
