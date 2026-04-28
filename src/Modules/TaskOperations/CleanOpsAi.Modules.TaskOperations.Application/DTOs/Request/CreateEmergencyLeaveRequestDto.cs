namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
    public class CreateEmergencyLeaveRequestDto
    {
        public Guid WorkerId { get; set; }
        public Guid? TaskAssignmentId { get; set; }
        public DateTime? LeaveDateFrom { get; set; } 
        public DateTime? LeaveDateTo { get; set; }  
        public Stream? AudioStream { get; set; }
        public string? AudioFileName { get; set; } 
        public string? Transcription { get; set; }
    }
}
