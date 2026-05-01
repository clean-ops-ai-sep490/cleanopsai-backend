using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
    public class IssueReportDto
    {
        public Guid Id { get; set; }
        public Guid TaskAssignmentId { get; set; }
        public string? TaskName { get; set; }
        public Guid ReportedByWorkerId { get; set; }
        public string? ReportedByWorkerName { get; set; }
        public string Description { get; set; } = null!;
        public IssueStatus Status { get; set; }
        public Guid? ResolvedByUserId { get; set; }
        public string? ResolvedByUserName { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastModified { get; set; }
        public string? DisplayLocation { get; set; }
    }
}
