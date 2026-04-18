using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
    public class ReviewEquipmentRequestDto
    {
        public EquipmentRequestStatus Status { get; set; } // Approved / Rejected
    }
}
