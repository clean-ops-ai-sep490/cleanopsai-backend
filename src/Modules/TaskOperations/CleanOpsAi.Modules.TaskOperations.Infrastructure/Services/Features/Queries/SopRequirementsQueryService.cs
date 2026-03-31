using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Services.Features.Queries
{
	public class SopRequirementsQueryService : ISopRequirementsQueryService
	{
		private readonly IRequestClient<SopRequirementsRequested> _client;
		private readonly IMemoryCache _cache;

		public SopRequirementsQueryService(
			IRequestClient<SopRequirementsRequested> client,
			IMemoryCache cache)
		{
			_client = client;
			_cache = cache;
		}

		public async Task<SopRequirementsIntegrated> GetSopRequirementsByScheduleId(
			Guid taskScheduleId,
			CancellationToken ct)
		{
			var cacheKey = $"sop-requirements-{taskScheduleId}";

			if (_cache.TryGetValue(cacheKey, out SopRequirementsIntegrated? cached))
				return cached!;

			var response = await _client.GetResponse<SopRequirementsIntegrated>(
				new SopRequirementsRequested { TaskScheduleId = taskScheduleId }, ct);

			_cache.Set(cacheKey, response.Message, TimeSpan.FromMinutes(30));

			return response.Message;
		}
	}
}
