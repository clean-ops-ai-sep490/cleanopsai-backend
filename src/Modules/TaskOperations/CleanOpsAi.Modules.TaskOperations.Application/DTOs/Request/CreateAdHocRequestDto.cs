using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
    public class CreateAdHocRequestDto
    {
        public Guid WorkAreaId { get; set; }

        public AdHocRequestType RequestType { get; set; }

        public DateTime RequestDateFrom { get; set; }

        public DateTime? RequestDateTo { get; set; }

        public string? Reason { get; set; }

        public string? Description { get; set; }
    }
}
