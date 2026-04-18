using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response
{
    public class GetWorkersByWorkAreaResponse
    {
        public List<WorkerDto> Workers { get; set; } = new();
    }
}
