using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
    public class EmergencyLeaveRequestDto
    {
        public Guid Id { get; set; }
        public Guid WorkerId { get; set; }
        public Guid TaskAssignmentId { get; set; }
        public string? AudioUrl { get; set; }
        public string? Transcription { get; set; }
        public RequestStatus Status { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
