using CleanOpsAi.Modules.ClientManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.Slas
{
    public class SlaResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public Guid? EnvironmentTypeId { get; set; }

        public ServiceType ServiceType { get; set; }

        public Guid WorkAreaId { get; set; }

        public string? WorkAreaName { get; set; }

        public Guid ContractId { get; set; }

        public string? ContractName { get; set; }

        public DateTime Created { get; set; }

        public DateTime? LastModified { get; set; }
    }
}
