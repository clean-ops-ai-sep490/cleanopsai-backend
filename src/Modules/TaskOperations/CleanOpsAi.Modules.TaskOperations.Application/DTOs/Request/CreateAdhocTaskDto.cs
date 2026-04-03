using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
    public class CreateAdhocTaskDto
    {
        public Guid AssigneeId { get; set; }
        public Guid WorkAreaId { get; set; }
        public DateTime StartAt { get; set; }
        public int DurationMinutes { get; set; }
        public string Name { get; set; }
    }
}
