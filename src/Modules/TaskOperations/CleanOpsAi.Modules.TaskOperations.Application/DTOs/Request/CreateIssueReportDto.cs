namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
    public class CreateIssueReportDto
    {
        public Guid TaskAssignmentId { get; set; }
        public Guid ReportedByWorkerId { get; set; }
        public string Description { get; set; } = null!;
    }
}
