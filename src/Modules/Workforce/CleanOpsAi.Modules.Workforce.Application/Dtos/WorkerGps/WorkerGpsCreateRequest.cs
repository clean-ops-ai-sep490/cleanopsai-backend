using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerGps
{
    public class WorkerGpsCreateRequest
    {
        public Guid WorkerId { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public bool? IsConfirmed { get; set; }
    }
}
