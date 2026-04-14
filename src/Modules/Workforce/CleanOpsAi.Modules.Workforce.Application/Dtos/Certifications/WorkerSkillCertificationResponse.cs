using CleanOpsAi.Modules.Workforce.Application.Dtos.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Certifications
{
    public class WorkerSkillCertificationResponse
    {
        public List<WorkerSkillResponse> Skills { get; set; } = new();
        public List<WorkerCertificationResponse> Certifications { get; set; } = new();
    }
}
