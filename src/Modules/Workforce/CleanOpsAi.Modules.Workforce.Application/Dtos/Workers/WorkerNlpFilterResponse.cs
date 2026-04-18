using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Workers
{
    public class WorkerNlpFilterResponse
    {
        public string? Warning { get; set; }
        public List<WorkerResponse> Workers { get; set; } = new();
    }
}
