using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response
{
    public class GetEquipmentsByIdsResponse
    {
        public Dictionary<Guid, string> Equipments { get; set; } = new();
    }
}
