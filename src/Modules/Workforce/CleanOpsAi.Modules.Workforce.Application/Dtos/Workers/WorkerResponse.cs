using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Workers
{
    public class WorkerResponse
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string FullName { get; set; } = null!;

        public string? DisplayAddress { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string AvatarUrl { get; set; } = null!;

        public int TotalSkills { get; set; }

        public int TotalCertifications { get; set; }

        public List<SkillDto> Skills { get; set; } = new();

        public List<CertificationDto> Certifications { get; set; } = new();
    }

    public class SkillDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Category { get; set; }
    }

    public class CertificationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Category { get; set; }
        public DateTime? ExpiredAt { get; set; }
    }
}
