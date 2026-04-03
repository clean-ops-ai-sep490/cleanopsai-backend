using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response
{
    public class GetWorkAreaByIdResponse
    {
        public Guid? WorkAreaId { get; set; }
        public string? WorkAreaName { get; set; }
        public bool Found { get; set; }
    }
}
