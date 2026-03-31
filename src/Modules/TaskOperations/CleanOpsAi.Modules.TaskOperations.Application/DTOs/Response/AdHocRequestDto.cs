using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
    public class AdHocRequestDto
    {
        public Guid Id { get; set; }

        public Guid TaskAssignmentId { get; set; }

        public DateTime RequestDateFrom { get; set; }

        public DateTime? RequestDateTo { get; set; }

        public Guid RequestedByWorkerId { get; set; }

        public string? WorkerName { get; set; }

        public AdHocRequestType RequestType { get; set; }

        public string? Reason { get; set; }

        public string? Description { get; set; }

        public AdHocRequestStatus Status { get; set; }

        public Guid? ReviewedByUserId { get; set; }

        public string? ReviewedByUserName { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime Created { get; set; }
    }
}
