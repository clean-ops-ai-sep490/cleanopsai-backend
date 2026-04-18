using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
    public class WorkerAvailabilityDto
    {
        public Guid WorkerId { get; set; }
        public string FullName { get; set; } = null!;
        public bool IsBusy { get; set; }
    }
}
