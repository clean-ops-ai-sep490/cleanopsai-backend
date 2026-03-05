using CleanOpsAi.Modules.UserAccess.Application.Contracts;
using CleanOpsAi.Modules.UserAccess.Application.Users.RegisterUserWithEmail;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CleanOpsAi.Modules.UserAccess.Infrastructure.Auth0
{
	public class Auth0Service : IAuth0Service
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;

		private string Domain => _configuration["Auth0:Domain"];
		private string ClientId => _configuration["Auth0:ClientId"];
		private string ClientSecret => _configuration["Auth0:ClientSecret"];
		private string Connection => "CleanOpsAiDB";

		public Auth0Service(HttpClient httpClient, IConfiguration configuration)
		{
			_httpClient = httpClient;
			_configuration = configuration;
		}

		public async Task<RegisterUserResult> Register(string email, string password, string fullName)
		{
			var mgmtToken = await GetManagementTokenAsync();

			var payload = new
			{
				email,
				password,
				name = fullName,
				connection = Connection
			};

			var request = new HttpRequestMessage(HttpMethod.Post,
		   $"{Domain}api/v2/users");
			request.Headers.Authorization =
				new AuthenticationHeaderValue("Bearer", mgmtToken);
			request.Content = JsonContent.Create(payload);

			var response = await _httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			var result = await response.Content.ReadFromJsonAsync<JsonElement>();

			return new RegisterUserResult
			{
				Auth0UserId = result.GetProperty("user_id").GetString(),
				Email = result.GetProperty("email").GetString()
			};
		}



		private async Task<string> GetManagementTokenAsync()
		{
			var payload = new
			{
				grant_type = "client_credentials",
				client_id = ClientId,
				client_secret = ClientSecret,
				audience = $"{Domain}api/v2/"
			};

			var response = await _httpClient.PostAsJsonAsync(
				$"{Domain}oauth/token", payload);
			response.EnsureSuccessStatusCode();

			var result = await response.Content.ReadFromJsonAsync<JsonElement>();
			return result.GetProperty("access_token").GetString();
		}
	}
}
