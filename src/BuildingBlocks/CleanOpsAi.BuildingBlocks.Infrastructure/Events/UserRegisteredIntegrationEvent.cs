using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events
{
    public record UserRegisteredIntegrationEvent
    {
        public string UserId { get; init; } = null!;

        public string Role { get; init; } = null!;

        public string FullName { get; init; } = null!;

        public string? AvatarUrl { get; init; }
    }
}
