using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
    public class UploadStepImagesRequest
    {
        public int MinPhotos { get; set; } = 1;
        public List<IFormFile> Images { get; set; } = new();
    }
}
