using CleanOpsAi.Modules.Workforce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerSkills
{
    public class WorkerSkillResponse
    {
        public Guid WorkerId { get; set; }

        public Guid SkillId { get; set; }

        public string WorkerName { get; set; } = null!;

        public string SkillName { get; set; } = null!;

        public SkillLevelType SkillLevel { get; set; }
    }
}
