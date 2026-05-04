using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
    public class CreateAdHocRequestDto
    {
        public Guid WorkAreaId { get; set; }

        public AdHocRequestType RequestType { get; set; }

        [Required]
        public DateTime RequestDateFrom { get; set; }

        public DateTime? RequestDateTo { get; set; }

        public string? Reason { get; set; }

        public string? Description { get; set; }
    }
}
