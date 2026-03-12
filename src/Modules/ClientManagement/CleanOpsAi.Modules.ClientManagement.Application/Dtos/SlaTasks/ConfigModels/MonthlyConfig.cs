using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.SlaTasks.ConfigModels
{
    public class MonthlyConfig
    {
        public int Month { get; set; }

        public int DayOfMonth { get; set; }

        public string Time { get; set; }
    }
}
