using CleanOpsAi.Modules.UserAccess.Domain;

namespace CleanOpsAi.Api.Modules.UserAccess.Dtos
{
    public class UpdateUserRequest
    {
        public string FullName { get; set; } = default!;
        public UserRole Role { get; set; }
    }
}
