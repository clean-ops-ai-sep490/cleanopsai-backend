using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
    public class UpdateEquipmentRequestDto
    {
        public int Quantity { get; set; }
        public string? Reason { get; set; }
    }
}
