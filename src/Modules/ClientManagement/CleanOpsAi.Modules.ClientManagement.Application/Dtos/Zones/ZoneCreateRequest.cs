using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.Zones
{
    public class ZoneCreateRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid LocationId { get; set; }
    }
}
