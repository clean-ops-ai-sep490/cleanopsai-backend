using System.Security.Claims;
using CleanOpsAi.BuildingBlocks.Application;
using Microsoft.AspNetCore.Http;

namespace CleanOpsAi.BuildingBlocks.Infrastructure
{
	public class UserContext : IUserContext
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public UserContext(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

		public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

		public Guid UserId
		{
			get
			{
				var sub = User?.FindFirst("sub")?.Value;
				return sub is not null ? Guid.Parse(sub) : Guid.Empty;
			}
		}

		public string Role => User?.FindFirst("role")?.Value ?? string.Empty;

		public string Email => User?.FindFirst("email")?.Value
			?? User?.FindFirst(ClaimTypes.Email)?.Value
			?? string.Empty;

		public string FullName => User?.FindFirst("fullName")?.Value ?? string.Empty;
	}
}
