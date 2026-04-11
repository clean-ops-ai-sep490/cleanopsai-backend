using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Services
{
	public class ScoringJobCache : IScoringJobCache
	{
		private static readonly JsonSerializerOptions SerializerOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};
		private readonly IDistributedCache _cache;

		public ScoringJobCache(IDistributedCache cache)
		{
			_cache = cache;
		}

		public async Task<ScoringJobDetailResponse?> GetAsync(Guid jobId, CancellationToken ct = default)
		{
			var cacheValue = await _cache.GetStringAsync(BuildKey(jobId), ct);
			if (string.IsNullOrWhiteSpace(cacheValue))
			{
				return null;
			}

			return JsonSerializer.Deserialize<ScoringJobDetailResponse>(cacheValue, SerializerOptions);
		}

		public async Task SetAsync(ScoringJobDetailResponse response, CancellationToken ct = default)
		{
			var content = JsonSerializer.Serialize(response, SerializerOptions);
			await _cache.SetStringAsync(
				BuildKey(response.JobId),
				content,
				new DistributedCacheEntryOptions
				{
					AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
				},
				ct);
		}

		private static string BuildKey(Guid jobId) => $"scoring-job:{jobId:N}";
	}
}
