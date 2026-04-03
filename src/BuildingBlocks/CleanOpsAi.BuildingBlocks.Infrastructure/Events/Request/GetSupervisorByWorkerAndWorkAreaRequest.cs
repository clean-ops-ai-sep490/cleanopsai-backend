using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
    public record GetSupervisorByWorkerAndWorkAreaRequest
    {
        public Guid WorkAreaId { get; init; }
        public Guid WorkerId { get; init; }
    }
}
