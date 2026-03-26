using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
    public class CreateIssueReportDto
    {
        public Guid TaskAssignmentId { get; set; }
        public Guid ReportedByWorkerId { get; set; }
        public string Description { get; set; } = null!;
    }
}
