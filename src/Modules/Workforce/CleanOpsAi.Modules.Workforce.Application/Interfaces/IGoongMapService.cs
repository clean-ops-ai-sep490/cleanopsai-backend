using CleanOpsAi.Modules.Workforce.Application.Dtos.Maps;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
	public interface IGoongMapService
	{
		Task<(double lat, double lng)?> GetCoordinatesAsync(string address);

		Task<List<GoongPlaceSuggestion>> GetPlaceSuggestionsAsync(string input, CancellationToken cancellationToken = default);

		Task<(double lat, double lng)?> GetCoordinatesByPlaceIdAsync(string placeId, CancellationToken cancellationToken = default);
	}
}
