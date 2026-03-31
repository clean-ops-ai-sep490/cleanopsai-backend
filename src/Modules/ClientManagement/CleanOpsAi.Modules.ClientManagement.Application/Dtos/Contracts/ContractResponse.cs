using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.Contracts
{
    public class ContractResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string UrlFile { get; set; } = null!;
        public Guid ClientId { get; set; }
        public string? ClientName { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
