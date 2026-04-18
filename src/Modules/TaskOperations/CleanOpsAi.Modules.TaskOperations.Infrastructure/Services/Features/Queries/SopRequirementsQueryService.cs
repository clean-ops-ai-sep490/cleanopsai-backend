using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Services.Features.Queries
{
	public class SopRequirementsQueryService : ISopRequirementsQueryService
	{
		private readonly IIntegrationBus _bus; 
		private readonly IMemoryCache _cache;

		public SopRequirementsQueryService(IIntegrationBus bus, IMemoryCache cache)
		{
			_bus = bus;
			_cache = cache;
		}

		public async Task<SopRequirementsIntegrated> GetSopRequirementsByScheduleId(Guid taskScheduleId, CancellationToken ct)
		{
			var cacheKey = $"sop-requirements-{taskScheduleId}";
			if (_cache.TryGetValue(cacheKey, out SopRequirementsIntegrated? cached)) return cached!;
			 
			var response = await _bus.RequestAsync<SopRequirementsRequested, SopRequirementsIntegrated>(
				new SopRequirementsRequested { TaskScheduleId = taskScheduleId }, ct);

			_cache.Set(cacheKey, response, TimeSpan.FromMinutes(30));
			return response;
		}
	}
}
