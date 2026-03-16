using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerGps
{
    public class WorkerGpsResponse
    {
        public Guid Id { get; set; }

        public Guid WorkerId { get; set; }

        public string WorkerName { get; set; } = null!;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public DateTime Created { get; set; }
    }
}
