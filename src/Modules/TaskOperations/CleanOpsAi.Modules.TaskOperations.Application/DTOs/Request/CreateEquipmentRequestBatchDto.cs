using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
    public class CreateEquipmentRequestBatchDto
    {
        public Guid TaskAssignmentId { get; set; }
        public Guid WorkerId { get; set; }
        public string? Reason { get; set; }

        public List<CreateEquipmentRequestItemDto> Items { get; set; } = new();
    }
}
