using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.Workareas
{
    public class WorkAreaCreateRequest
    {
        public string Name { get; set; } = null!;
        public Guid ZoneId { get; set; }
    }
}
