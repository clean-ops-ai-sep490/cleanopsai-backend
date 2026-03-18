using CleanOpsAi.Modules.Workforce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Equipments
{
    public class EquipmentResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public EquipmentType Type { get; set; }

        public string? Description { get; set; }
    }
}
