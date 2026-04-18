using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Workers
{
    public class WorkerUpdateRequest
    {
        public string? FullName { get; set; }
        public string? DisplayAddress { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public Stream? AvatarStream { get; set; }
        public string? AvatarFileName { get; set; }
    }
}
