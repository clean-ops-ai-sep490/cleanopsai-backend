using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
    public class UploadStepImagesResponse
    {
        public string Phase { get; set; } = null!;
        public int MinPhotos { get; set; }
        public int ActualPhotos { get; set; }
    }
}
