using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
    public class GetWorkerIdByUserIdRequest
    {
        public Guid UserId { get; set; }
    }
}
