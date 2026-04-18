using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response
{
    public class GetBusyWorkerIdsResponse
    {
        public List<Guid> BusyWorkerIds { get; set; } = new();
    }
}
