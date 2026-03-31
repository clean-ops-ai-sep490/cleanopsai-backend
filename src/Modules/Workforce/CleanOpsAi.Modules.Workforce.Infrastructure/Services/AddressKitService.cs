using CleanOpsAi.Modules.Workforce.Application.Interfaces;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Services
{
	public class AddressKitService : IAddressKitService
	{
		private readonly HttpClient _httpClient;
		public AddressKitService(HttpClient httpClient) => _httpClient = httpClient;

		public async Task<string> GetProvincesAsync(CancellationToken ct)
		{
			var response = await _httpClient.GetAsync("provinces", ct);
			 
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadAsStringAsync(ct);
		}

		public async Task<string> GetCommunesAsync(string code, CancellationToken ct)
		{
			var response = await _httpClient.GetAsync($"provinces/{Uri.EscapeDataString(code)}/communes", ct);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsStringAsync(ct);
		}
	}
}
