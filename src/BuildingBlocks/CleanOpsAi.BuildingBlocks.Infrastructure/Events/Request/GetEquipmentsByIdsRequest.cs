using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
    public class GetEquipmentsByIdsRequest
    {
        public List<Guid> EquipmentIds { get; set; } = new();
    }
}
