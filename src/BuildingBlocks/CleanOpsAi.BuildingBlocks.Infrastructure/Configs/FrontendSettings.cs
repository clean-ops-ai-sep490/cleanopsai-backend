using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Configs
{
    public class FrontendSettings
    {
        public string BaseUrl { get; set; } = default!;
        public string ResetPasswordPath { get; set; } = default!;
    }
}
