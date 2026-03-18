using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerCertifications
{
    public class WorkerCertificationResponse
    {
        public Guid WorkerId { get; set; }

        public Guid CertificationId { get; set; }

        public string WorkerName { get; set; } = null!;

        public string CertificationName { get; set; } = null!;

        public DateTime IssuedDate { get; set; }

        public DateTime? ExpiredAt { get; set; }
    }
}
