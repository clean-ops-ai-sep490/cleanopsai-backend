using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
    public class CreateEquipmentRequestItemDto
    {
        public Guid EquipmentId { get; set; }
        public int Quantity { get; set; }
    }
}
