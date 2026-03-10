using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.Clients
{
    public class ClientUpdateRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
