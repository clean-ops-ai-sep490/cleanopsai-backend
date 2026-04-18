using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.PpeItems
{
    public class PpeItemUpdateRequest
    {
        public string? ActionKey { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Stream? ImageStream { get; set; }
        public string? ImageFileName { get; set; }
    }
}
