using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
    public class EquipmentRequestDto
    {
        public Guid Id { get; set; }
        public Guid TaskAssignmentId { get; set; }
        public Guid WorkerId { get; set; }
        public Guid EquipmentId { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public int Quantity { get; set; }
        public string? Reason { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public EquipmentRequestStatus Status { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
