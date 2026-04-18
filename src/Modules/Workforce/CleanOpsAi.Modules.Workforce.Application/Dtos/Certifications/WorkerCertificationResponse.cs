using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Certifications
{
    public class WorkerCertificationResponse
    {
        public Guid CertificationId { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public string IssuingOrganization { get; set; }

        public DateTime IssuedDate { get; set; }

        public DateTime? ExpiredAt { get; set; }
    }
}
