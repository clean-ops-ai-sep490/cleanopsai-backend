using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response
{
    public record GetSupervisorByWorkerAndWorkAreaResponse
    {
        public Guid? SupervisorId { get; init; }
        public string? SupervisorName { get; init; }
        public bool Found { get; init; }
    }
}
