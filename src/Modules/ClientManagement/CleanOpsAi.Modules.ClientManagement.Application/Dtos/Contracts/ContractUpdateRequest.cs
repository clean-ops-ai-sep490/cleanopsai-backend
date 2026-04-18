using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.Contracts
{
    public class ContractUpdateRequest
    {
        public string? Name { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public Stream? FileStream { get; set; }
        public string? FileName { get; set; }
    }
}
