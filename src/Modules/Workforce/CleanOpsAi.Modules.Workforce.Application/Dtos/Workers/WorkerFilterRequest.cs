using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Workers
{
    public class WorkerFilterRequest
    {
        public string? Address { get; set; }          // FE truyền vào
        public double? Latitude { get; set; }         // internal, FE không cần truyền
        public double? Longitude { get; set; }        // internal, FE không cần truyền
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public List<Guid>? CertificateIds { get; set; }
        public List<Guid>? SkillIds { get; set; }
    }
}
