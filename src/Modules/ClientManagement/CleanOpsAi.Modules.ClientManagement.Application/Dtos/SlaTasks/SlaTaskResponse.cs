using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.SlaTasks
{
    public class SlaTaskResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid SlaId { get; set; }

        public string SlaName { get; set; }

        public string RecurrenceType { get; set; }

        public string RecurrenceConfig { get; set; }
    }
}
