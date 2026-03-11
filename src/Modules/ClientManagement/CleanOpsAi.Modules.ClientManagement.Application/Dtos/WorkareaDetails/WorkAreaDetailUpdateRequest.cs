using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.WorkareaDetails
{
    public class WorkAreaDetailUpdateRequest
    {
        public string Name { get; set; } = null!;
        public double Area { get; set; }
        public double TotalArea { get; set; }
    }
}
