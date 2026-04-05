using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanOpsAi.Api.Modules.Workforce
{
	[Route("api/[controller]")]
	[ApiController]
	public class AddressController : ControllerBase
	{
		private readonly IAddressKitService _addressService;
		private readonly IGoongMapService _goongMapService;


		// DI sẽ tự bơm AddressKitService vào đây
		public AddressController(IAddressKitService addressService, IGoongMapService goongMapService)
		{
			_addressService = addressService;
			_goongMapService = goongMapService;
		}

		[HttpGet("provinces")]
		[Produces("application/json")]
		[SwaggerOperation(
			Summary = "Get provinces",
			Description = "Fetches the list of provinces from the external Address Kit service.",
			Tags = new[] { "Address" })]
		[SwaggerResponse(StatusCodes.Status200OK, "Provinces returned (raw JSON)")]
		[SwaggerResponse(StatusCodes.Status502BadGateway, "Upstream service returned an error")]
		public async Task<IActionResult> GetProvinces(CancellationToken ct)
		{
			try
			{
				var json = await _addressService.GetProvincesAsync(ct);
				return Content(json, "application/json");
			}
			catch (HttpRequestException ex)
			{
				return StatusCode(StatusCodes.Status502BadGateway, new { message = "Error connecting to third-party API", detail = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "System error", detail = ex.Message });
			}
		}

		[HttpGet("communes")]
		[Produces("application/json")]
		[SwaggerOperation(
			Summary = "Get communes by province code",
			Description = "Fetches communes for the specified provinceCode from the external Address Kit service.",
			Tags = new[] { "Address" })]
		[SwaggerResponse(StatusCodes.Status200OK, "Communes returned (raw JSON)")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "provinceCode is missing or invalid")]
		[SwaggerResponse(StatusCodes.Status502BadGateway, "Upstream service returned an error")]
		public async Task<IActionResult> GetCommunes([FromQuery] string provinceCode, CancellationToken ct)
		{
			if (string.IsNullOrWhiteSpace(provinceCode))
				return BadRequest(new { message = "provinceCode is required" });

			try
			{
				var json = await _addressService.GetCommunesAsync(provinceCode, ct);
				return Content(json, "application/json");
			}
			catch (HttpRequestException ex)
			{
				return StatusCode(StatusCodes.Status502BadGateway, new { message = "Lỗi kết nối API bên thứ ba", detail = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Lỗi hệ thống", detail = ex.Message });
			}
		}

		[HttpGet("geocode")]
		[Produces("application/json")]
		[SwaggerOperation(
			Summary = "Geocode address",
			Description = "Converts an address string to geographic coordinates (latitude, longitude).",
			Tags = new[] { "Address" })]
		[SwaggerResponse(StatusCodes.Status200OK, "Coordinates returned")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "Address parameter is missing or invalid")]
		[SwaggerResponse(StatusCodes.Status404NotFound, "Address not found")]
		public async Task<IActionResult> GeocodeAddress([FromQuery] string address, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(address))
				return BadRequest(new { message = "address parameter is required" });

			try
			{
				var coordinates = await _goongMapService.GetCoordinatesAsync(address);

				if (coordinates.HasValue)
					return Ok(new { lat = coordinates.Value.lat, lng = coordinates.Value.lng });

				return NotFound(new { message = "Address not found" });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to geocode address", detail = ex.Message });
			}
		}

		/// <summary>
		/// Get place suggestions (autocomplete) for an input string.
		/// </summary>
		[HttpGet("place-suggestions")]
		[Produces("application/json")]
		[SwaggerOperation(
			Summary = "Get place suggestions",
			Description = "Returns a list of place suggestions based on the input string using Goong Maps Autocomplete API.",
			Tags = new[] { "Address" })]
		[SwaggerResponse(StatusCodes.Status200OK, "Place suggestions returned")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "Input parameter is missing")]
		public async Task<IActionResult> GetPlaceSuggestions([FromQuery] string input, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(input))
				return BadRequest(new { message = "input parameter is required" });

			try
			{
				var suggestions = await _goongMapService.GetPlaceSuggestionsAsync(input, cancellationToken);
				return Ok(suggestions);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to get place suggestions", detail = ex.Message });
			}
		}

		[HttpGet("place-detail")]
		[Produces("application/json")]
		[SwaggerOperation(
			Summary = "Get place detail by PlaceId",
			Description = "Returns coordinates (lat, lng) of a place using its PlaceId from Goong Maps.",
			Tags = new[] { "Address" })]
		[SwaggerResponse(StatusCodes.Status200OK, "Place detail returned")]
		[SwaggerResponse(StatusCodes.Status400BadRequest, "PlaceId parameter is missing")]
		[SwaggerResponse(StatusCodes.Status404NotFound, "Place not found")]
		public async Task<IActionResult> GetPlaceDetail([FromQuery] string placeId, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(placeId))
				return BadRequest(new { message = "placeId parameter is required" });
			try
			{
				var coordinates = await _goongMapService.GetCoordinatesByPlaceIdAsync(placeId, cancellationToken);

				if (coordinates.HasValue)
					return Ok(new { lat = coordinates.Value.lat, lng = coordinates.Value.lng });

				return NotFound(new { message = "Place not found" });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to get place detail", detail = ex.Message });
			}
		}
	}
}
