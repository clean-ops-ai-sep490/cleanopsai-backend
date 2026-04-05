using System.Text.Json.Serialization;

namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Maps
{
	public class GoongAutocompleteResponse
	{
		public List<Prediction>? Predictions { get; set; }
		public string? Status { get; set; }
	}

	public class Prediction
	{
		[JsonPropertyName("description")]
		public string? Description { get; set; }

		[JsonPropertyName("place_id")]
		public string? PlaceId { get; set; }
	}

	public class GoongPlaceSuggestion
	{
		public string Description { get; set; } = string.Empty;

		[JsonPropertyName("place_id")]
		public string PlaceId { get; set; } = string.Empty;
	}
}
