using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerCertifications
{
    public class WorkerCertificationUpdateRequest
    {
        public DateTime IssuedDate { get; set; }

        public DateTime? ExpiredAt { get; set; }
    }
}
