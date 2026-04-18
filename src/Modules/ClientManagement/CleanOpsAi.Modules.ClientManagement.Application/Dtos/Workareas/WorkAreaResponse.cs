using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.Workareas
{
    public class WorkAreaResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid ZoneId { get; set; }
        public string? ZoneName { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
