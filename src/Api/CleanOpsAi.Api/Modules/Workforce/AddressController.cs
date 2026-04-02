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

		// DI sẽ tự bơm AddressKitService vào đây
		public AddressController(IAddressKitService addressService)
		{
			_addressService = addressService;
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
	}
}
