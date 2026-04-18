using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Skills
{
    public class SkillCreateRequest
    {
        public string Name { get; set; }
        public string Category { get; set; } = null!;
        public string? Description { get; set; }
    }
}
