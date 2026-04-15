using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Response;
using CleanOpsAi.Modules.Scoring.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Services
{
	public class ScoringInferenceClient : IScoringInferenceClient
	{
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNameCaseInsensitive = true
		};
		private readonly HttpClient _httpClient;
		private readonly ScoringServiceOptions _options;

		public ScoringInferenceClient(HttpClient httpClient, IOptions<ScoringServiceOptions> options)
		{
			_httpClient = httpClient;
			_options = options.Value;
		}

		public async Task<ScoringInferenceBatchResponse> EvaluateBatchAsync(string environmentKey, IReadOnlyCollection<string> imageUrls, CancellationToken ct = default)
		{
			using var form = new MultipartFormDataContent();
			foreach (var imageUrl in imageUrls)
			{
				form.Add(new StringContent(imageUrl), "image_urls");
			}
			form.Add(new StringContent(environmentKey), "env");

			using var request = new HttpRequestMessage(HttpMethod.Post, _options.EvaluateBatchPath)
			{
				Content = form,
			};
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			using var response = await _httpClient.SendAsync(request, ct);
			var body = await response.Content.ReadAsStringAsync(ct);
			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException($"Scoring service returned {(int)response.StatusCode}: {body}");
			}

			var payload = JsonSerializer.Deserialize<ScoringInferenceBatchResponse>(body, JsonOptions);
			return payload ?? throw new InvalidOperationException("Scoring service returned empty payload.");
		}

		public async Task<ScoringVisualizationLinkResponse> EvaluateUrlVisualizeLinkAsync(string environmentKey, string imageUrl, CancellationToken ct = default)
		{
			var requestPayload = JsonSerializer.Serialize(new
			{
				url = imageUrl,
				env = environmentKey,
			});

			using var request = new HttpRequestMessage(HttpMethod.Post, _options.EvaluateUrlVisualizeLinkPath)
			{
				Content = new StringContent(requestPayload, Encoding.UTF8, "application/json"),
			};
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			using var response = await _httpClient.SendAsync(request, ct);
			var body = await response.Content.ReadAsStringAsync(ct);
			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException($"Scoring visualization service returned {(int)response.StatusCode}: {body}");
			}

			var payload = JsonSerializer.Deserialize<ScoringVisualizationLinkResponse>(body, JsonOptions);
			return payload ?? throw new InvalidOperationException("Scoring visualization service returned empty payload.");
		}
	}
}
