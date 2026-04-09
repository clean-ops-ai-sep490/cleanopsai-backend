using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Nlps;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"]!;
        }

        public async Task<string> ChatAsync(string message)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = message } }
                    }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new BadRequestException($"Gemini API lỗi: {response.StatusCode}. Vui lòng thử lại.");

            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("candidates", out var candidates)
                || candidates.GetArrayLength() == 0)
                throw new BadRequestException("Gemini không trả về kết quả, vui lòng thử lại.");

            var result = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return result ?? "No response";
        }

        public async Task<WorkerFilterNlpResult> ParseWorkerFilterAsync(string query)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            var now = DateTime.UtcNow;
            var nowStr = now.ToString("yyyy-MM-ddTHH:mm:ss");
            var todayStr = now.ToString("yyyy-MM-dd");
            var tomorrowStr = now.AddDays(1).ToString("yyyy-MM-dd");

            var jsonStructure = """
                {
                    "address": "extracted address or null",
                    "skillCategories": ["skill1", "skill2"] or [],
                    "certificateCategories": ["cert1", "cert2"] or [],
                    "startAt": "ISO 8601 datetime or null",
                    "endAt": "ISO 8601 datetime or null"
                }
                """;

            var example1 = $$$"""{"address": "District 1", "skillCategories": ["glass cleaning"], "certificateCategories": [], "startAt": "{todayStr}T08:00:00", "endAt": "{todayStr}T10:00:00"}""";
            var example2 = $$$"""{"address": "Quan 3", "skillCategories": ["cleaning"], "certificateCategories": [], "startAt": "{tomorrowStr}T13:00:00", "endAt": null}""";
            var example3 = """{"address": "Ho Chi Minh City", "skillCategories": [], "certificateCategories": ["fire safety"], "startAt": null, "endAt": null}""";

            var prompt = $"""
                Extract worker search parameters from this query and return ONLY a JSON object, no explanation, no markdown.
                Current datetime (UTC): {nowStr}
                Query: "{query}"

                Return JSON with this exact structure:
                {jsonStructure}

                Rules for startAt/endAt:
                - If query mentions time range, extract it. Example: "from 8am to 10am today" → startAt: today 08:00, endAt: today 10:00
                - If only start time mentioned without end, set endAt null
                - If no time mentioned, both null
                - Always use ISO 8601 format: "yyyy-MM-ddTHH:mm:ss"
                - Use current datetime as reference for relative times like "today", "tomorrow", "this afternoon"
                - "this morning" = 08:00, "this afternoon" = 13:00, "this evening" = 18:00
                - Support Vietnamese time expressions: "sáng" = 08:00, "chiều" = 13:00, "tối" = 18:00
                - Support Vietnamese relative times: "hôm nay" = today, "ngày mai" = tomorrow

                Examples:
                - "Find workers with glass cleaning in District 1 from 8am to 10am today" → {example1}
                - "Available cleaners in Quan 3 tomorrow afternoon" → {example2}
                - "Workers with fire safety certificate near HCMC" → {example3}
                """;

            var body = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            // Retry tối đa 3 lần khi 503
            HttpResponseMessage response = null!;
            string json = string.Empty;
            int maxRetry = 3;

            for (int i = 0; i < maxRetry; i++)
            {
                response = await _httpClient.PostAsync(url, requestContent);
                json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    break;

                if ((int)response.StatusCode == 503)
                {
                    if (i < maxRetry - 1)
                        await Task.Delay(1000 * (i + 1)); // 1s, 2s, 3s
                    continue;
                }

                // Lỗi khác thì throw luôn
                throw new BadRequestException($"Gemini API lỗi: {response.StatusCode}. Vui lòng thử lại.");
            }

            if (!response.IsSuccessStatusCode)
                throw new BadRequestException("Gemini API hiện không khả dụng, vui lòng thử lại sau.");

            try
            {
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("candidates", out var candidates)
                    || candidates.GetArrayLength() == 0)
                    throw new BadRequestException("Gemini không trả về kết quả, vui lòng thử lại.");

                var rawText = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "{}";

                var cleaned = rawText
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                return JsonSerializer.Deserialize<WorkerFilterNlpResult>(cleaned, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new WorkerFilterNlpResult();
            }
            catch (JsonException)
            {
                throw new BadRequestException("Không thể parse kết quả từ Gemini, vui lòng thử lại.");
            }
        }
    }
}