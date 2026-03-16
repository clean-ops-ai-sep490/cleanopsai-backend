using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerCertifications
{
    public class WorkerCertificationCreateRequest
    {
        public Guid WorkerId { get; set; }

        public Guid CertificationId { get; set; }

        public DateTime IssuedDate { get; set; }

        public DateTime? ExpiredAt { get; set; }
    }
}
