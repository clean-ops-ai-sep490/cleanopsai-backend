using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Certifications
{
    public class CertificationCreateRequest
    {
        public string Name { get; set; }

        public string Category { get; set; } = null!;
        public string IssuingOrganization { get; set; }
    }
}
