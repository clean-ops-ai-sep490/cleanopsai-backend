using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Workers
{
    public class WorkerFilterRequest
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public List<string>? CertificateCategories { get; set; }
        public List<string>? SkillCategories { get; set; }
    }
}
