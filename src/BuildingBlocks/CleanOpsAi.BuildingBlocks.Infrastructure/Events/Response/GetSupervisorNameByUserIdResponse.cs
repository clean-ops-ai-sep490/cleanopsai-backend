using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response
{
    public class GetSupervisorNameByUserIdResponse
    {
        public string? FullName { get; set; }
        public bool Found { get; set; }
    }
}
