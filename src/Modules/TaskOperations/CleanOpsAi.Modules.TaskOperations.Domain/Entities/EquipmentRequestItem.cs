using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Domain.Entities
{
    [Table("equipment_request_items")]
    public class EquipmentRequestItem
    {
        public Guid Id { get; set; }

        public Guid EquipmentRequestId { get; set; }
        public EquipmentRequest EquipmentRequest { get; set; }

        public Guid EquipmentId { get; set; }

        public int Quantity { get; set; }
    }
}
