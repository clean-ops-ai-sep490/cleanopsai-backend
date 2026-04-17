using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors
{
    public class WorkAreaWithLocationResponse
    {
        public Guid WorkAreaId { get; set; }
        public string WorkAreaName { get; set; }

        public string ZoneName { get; set; }
        public string LocationName { get; set; }

        public string DisplayLocation { get; set; }
    }
}
