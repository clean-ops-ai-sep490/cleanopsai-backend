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

        private string BuildUrl()
        {
            return $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";
        }

        public async Task<string> ChatAsync(string message)
        {
            message = Sanitize(message);

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = message }
                        }
                    }
                }
            };

            var json = await SendWithRetry(body);
            return ExtractText(json);
        }

        public async Task<WorkerFilterNlpResult> ParseWorkerFilterAsync(string query)
        {
            query = Sanitize(query);

            var now = DateTime.UtcNow;
            var nowStr = now.ToString("yyyy-MM-ddTHH:mm:ss");
            var todayStr = now.ToString("yyyy-MM-dd");

            var prompt = $$"""
                Extract worker search parameters from this query and return ONLY raw JSON, no markdown, no explanation.

                Current datetime (UTC): {{nowStr}}
                Query: "{{query}}"

                JSON format:
                {
                  "address": string or null,
                  "skillCategories": string[],
                  "certificateCategories": string[],
                  "startAt": ISO datetime or null,
                  "endAt": ISO datetime or null
                }

                Rules:
                - "sáng" = 08:00, "chiều" = 13:00, "tối" = 18:00
                - "hôm nay" = {{todayStr}}
                - Nếu thiếu start hoặc end → set cả 2 = null
                - skillCategories và certificateCategories phải là lowercase, ví dụ: "cleaning", "glass cleaning"
                - Chỉ trả về JSON, không giải thích gì thêm

                Example output:
                {
                  "address": "District 1",
                  "skillCategories": ["glass cleaning"],
                  "certificateCategories": [],
                  "startAt": "{{todayStr}}T08:00:00",
                  "endAt": "{{todayStr}}T10:00:00"
                }
                """;

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            var json = await SendWithRetry(body);
            var rawText = ExtractText(json);
            var cleaned = CleanJson(rawText);

            try
            {
                var result = JsonSerializer.Deserialize<WorkerFilterNlpResult>(
                    cleaned,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result == null) return new WorkerFilterNlpResult();

                // ✅ Normalize về lowercase để khớp DB
                if (result.SkillCategories != null)
                    result.SkillCategories = result.SkillCategories
                        .Select(s => s.Trim().ToLowerInvariant())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();

                if (result.CertificateCategories != null)
                    result.CertificateCategories = result.CertificateCategories
                        .Select(s => s.Trim().ToLowerInvariant())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();

                return result;
            }
            catch
            {
                return new WorkerFilterNlpResult();
            }
        }

        // ================= CORE =================

        private async Task<string> SendWithRetry(object body)
        {
            var url = BuildUrl();
            HttpResponseMessage response = null!;
            string json = "";

            int maxRetry = 3;

            for (int i = 0; i < maxRetry; i++)
            {
                var requestContent = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"
                );

                response = await _httpClient.PostAsync(url, requestContent);
                json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return json;

                if ((int)response.StatusCode == 503 || (int)response.StatusCode == 400)
                {
                    await Task.Delay(1000 * (i + 1));
                    continue;
                }

                throw new BadRequestException(
                    $"Gemini API lỗi: {response.StatusCode} - {json}"
                );
            }

            throw new BadRequestException(
                $"Gemini API không khả dụng sau retry. Response: {json}"
            );
        }

        private string ExtractText(string json)
        {
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("candidates", out var candidates)
                || candidates.GetArrayLength() == 0)
                return "{}";

            return candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "{}";
        }

        private string CleanJson(string raw)
        {
            var cleaned = raw
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var start = cleaned.IndexOf('{');
            var end = cleaned.LastIndexOf('}');

            if (start >= 0 && end > start)
                cleaned = cleaned.Substring(start, end - start + 1);

            return cleaned;
        }

        private string Sanitize(string input)
        {
            return input
                .Replace("\"", "'")
                .Trim();
        }
    }
}