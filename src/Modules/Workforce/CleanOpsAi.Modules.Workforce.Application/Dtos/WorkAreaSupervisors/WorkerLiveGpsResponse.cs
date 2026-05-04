using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors
{
    public class WorkerLiveGpsResponse
    {
        public Guid WorkerId { get; set; }
        public string? WorkerName { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public bool IsOnline { get; set; }
        public bool? IsConfirmed { get; set; }

        public DateTime LastSeen { get; set; }
    }
}
