using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.PpeItems
{
    public class PpeItemCreateRequest
    {
        public string ActionKey { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Stream? ImageStream { get; set; }
        public string? ImageFileName { get; set; }
    }
}
