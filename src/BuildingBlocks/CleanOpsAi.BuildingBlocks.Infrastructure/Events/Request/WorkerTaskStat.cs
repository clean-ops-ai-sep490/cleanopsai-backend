using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
    public class WorkerTaskStat
    {
        public Guid WorkerId { get; set; }
        public int TotalTasks { get; set; }
    }
}
