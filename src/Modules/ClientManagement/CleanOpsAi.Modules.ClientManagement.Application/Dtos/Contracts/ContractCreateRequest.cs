using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.Contracts
{
    public class ContractCreateRequest
    {
        public string Name { get; set; }
        public Guid ClientId { get; set; }
        public Stream? FileStream { get; set; }
        public string? FileName { get; set; }
    }
}
