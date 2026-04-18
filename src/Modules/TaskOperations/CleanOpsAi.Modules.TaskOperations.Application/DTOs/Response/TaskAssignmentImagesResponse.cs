using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
    public class TaskAssignmentImagesResponse
    {
        public Guid TaskAssignmentId { get; set; }
        public List<StepImagesDto> Steps { get; set; } = new();
    }

    public class StepImagesDto
    {
        public Guid StepExecutionId { get; set; }
        public int StepOrder { get; set; }
        public TaskStepExecutionStatus Status { get; set; }
        public List<StepImageDto> Images { get; set; } = new();
    }

    public class StepImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = null!;
        public ImageType ImageType { get; set; }
        public string Phase => ImageType.ToString().ToLower(); // "before" / "after" / "ppe"
    }
}
