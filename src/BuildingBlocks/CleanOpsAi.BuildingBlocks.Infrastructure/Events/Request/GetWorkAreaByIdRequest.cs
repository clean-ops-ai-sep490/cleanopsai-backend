using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
    public class GetWorkAreaByIdRequest
    {
        public Guid WorkAreaId { get; set; }
    }
}
