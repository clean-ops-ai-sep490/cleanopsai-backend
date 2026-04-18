using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
    public class CreateEmergencyLeaveRequestDto
    {
        public Guid WorkerId { get; set; }
        public Guid? TaskAssignmentId { get; set; }
        public DateTime? LeaveDateFrom { get; set; }  // bat buoc neu khong co TaskAssignmentId
        public DateTime? LeaveDateTo { get; set; }  // bat buoc neu khong co TaskAssignmentId
        public Stream? AudioStream { get; set; }
        public string? AudioFileName { get; set; } // lay tu IFormFile.FileName o controller
        public string? Transcription { get; set; }
    }
}
