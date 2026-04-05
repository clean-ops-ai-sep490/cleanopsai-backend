using CleanOpsAi.Modules.Workforce.Application.Dtos.Maps;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Services
{
	public class GoongMapService : IGoongMapService
	{
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;

		public GoongMapService(HttpClient httpClient, IConfiguration config)
		{
			_httpClient = httpClient;
			_apiKey = config["Goong:ApiKey"]!;
		}

		public async Task<(double lat, double lng)?> GetCoordinatesAsync(string address)
		{
			var url = $"https://rsapi.goong.io/geocode?address={Uri.EscapeDataString(address)}&api_key={_apiKey}";
			var response = await _httpClient.GetFromJsonAsync<GoongMapDto>(url);
			var location = response?.Results?.FirstOrDefault()?.Geometry?.Location;
			return location is not null ? (location.Lat, location.Lng) : null;
		}

		public async Task<(double lat, double lng)?> GetCoordinatesByPlaceIdAsync(
			string placeId,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(placeId))
				return null;

			var url = $"https://rsapi.goong.io/Place/Detail?place_id={Uri.EscapeDataString(placeId)}&api_key={_apiKey}";
			var response = await _httpClient.GetFromJsonAsync<GoongPlaceDetailResponse>(url, cancellationToken);

			var location = response?.Result?.Geometry?.Location;
			return location is not null ? (location.Lat, location.Lng) : null;
		}

		public async Task<List<GoongPlaceSuggestion>> GetPlaceSuggestionsAsync(string input, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(input))
				return new List<GoongPlaceSuggestion>();

			var url = $"https://rsapi.goong.io/Place/AutoComplete?api_key={_apiKey}&input={Uri.EscapeDataString(input)}";
			var response = await _httpClient.GetFromJsonAsync<GoongAutocompleteResponse>(url, cancellationToken);
			return response?.Predictions?
				.Select(p => new GoongPlaceSuggestion
				{
					Description = p.Description ?? string.Empty,
					PlaceId = p.PlaceId ?? string.Empty
				})
				.ToList() ?? new List<GoongPlaceSuggestion>();
		}
	}
}
