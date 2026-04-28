using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
    public class GetWorkAreasByIdsRequest
    {
        public List<Guid> WorkAreaIds { get; set; } = new();
    }

    public class GetWorkAreasByIdsResponse
    {
        public List<WorkAreaWithLocationDto> Items { get; set; } = new();
    }

    public class WorkAreaWithLocationDto
    {
        public Guid WorkAreaId { get; set; }
        public string WorkAreaName { get; set; }

        public string ZoneName { get; set; }
        public string LocationName { get; set; }

        public string DisplayLocation { get; set; }
    }
}
