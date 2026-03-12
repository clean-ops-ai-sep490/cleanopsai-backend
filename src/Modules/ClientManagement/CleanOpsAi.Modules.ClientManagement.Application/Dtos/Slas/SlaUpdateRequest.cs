using CleanOpsAi.Modules.ClientManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.Slas
{
    public class SlaUpdateRequest
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public EnvironmentType? EnvironmentType { get; set; }

        public ServiceType? ServiceType { get; set; }
    }
}
