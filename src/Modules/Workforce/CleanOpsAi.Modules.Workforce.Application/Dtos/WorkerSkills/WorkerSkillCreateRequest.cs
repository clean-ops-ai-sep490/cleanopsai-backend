using CleanOpsAi.Modules.Workforce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerSkills
{
    public class WorkerSkillCreateRequest
    {
        public Guid WorkerId { get; set; }

        public Guid SkillId { get; set; }

        public SkillLevelType SkillLevel { get; set; }
    }
}
