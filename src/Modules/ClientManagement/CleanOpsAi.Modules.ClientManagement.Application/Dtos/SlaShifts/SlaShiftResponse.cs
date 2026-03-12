using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.SlaShifts
{
    public class SlaShiftResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid SlaId { get; set; }

        public string SlaName { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public int RequiredWorker { get; set; }

        public int BreakTime { get; set; }
    }
}
