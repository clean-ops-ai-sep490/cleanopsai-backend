using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Nlps
{
    public class WorkerFilterNlpResult
    {
        public string? Address { get; set; }
        public List<string> SkillCategories { get; set; } = new();
        public List<string> CertificateCategories { get; set; } = new();
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
    }
}
