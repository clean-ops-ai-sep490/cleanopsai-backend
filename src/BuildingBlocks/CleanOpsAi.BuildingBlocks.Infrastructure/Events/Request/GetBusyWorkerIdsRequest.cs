using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
    public class GetBusyWorkerIdsRequest
    {
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
    }
}
