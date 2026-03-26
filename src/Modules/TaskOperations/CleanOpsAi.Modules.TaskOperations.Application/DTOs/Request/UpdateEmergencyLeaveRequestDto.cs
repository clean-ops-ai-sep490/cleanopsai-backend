using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
    public class UpdateEmergencyLeaveRequestDto
    {
        public Stream? AudioStream { get; set; }
        public string? AudioFileName { get; set; } // lay tu IFormFile.FileName o controller
        public string? Transcription { get; set; }
    }
}
