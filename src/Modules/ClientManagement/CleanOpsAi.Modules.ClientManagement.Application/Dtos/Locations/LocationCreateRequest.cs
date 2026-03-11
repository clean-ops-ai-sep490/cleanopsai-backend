using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.Locations
{
    public class LocationCreateRequest
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string? Street { get; set; }
        public string? Commune { get; set; }
        public string? Province { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public Guid ClientId { get; set; }
    }
}
