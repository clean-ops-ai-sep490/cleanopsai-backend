using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Nlps;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Services;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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

        // =====================================================
        // CHAT
        // =====================================================
        public async Task<string> ChatAsync(string message)
        {
            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = message?.Trim() ?? "" }
                        }
                    }
                }
            };

            try
            {
                var json = await Send(body);
                return ExtractText(json);
            }
            catch (TaskCanceledException)
            {
                throw new BadRequestException("Gemini request timeout. Vui lòng thử lại sau.");
            }
        }

        // =====================================================
        // NLP
        // =====================================================
        public async Task<WorkerFilterNlpResult> ParseWorkerFilterAsync(string query, CancellationToken ct = default)
        {
            query = query?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(_apiKey))
                return new WorkerFilterNlpResult();

            try
            {
                var prompt = BuildPrompt(query);

                var body = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.1,
                        maxOutputTokens = 512,
                        responseMimeType = "application/json"
                    }
                };

                var json = await Send(body, ct);

                var raw = ExtractText(json);
                var cleaned = CleanJson(raw);

                var result = JsonSerializer.Deserialize<WorkerFilterNlpResult>(
                    cleaned,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return result ?? new WorkerFilterNlpResult();
            }
            catch (TaskCanceledException)
            {
                return new WorkerFilterNlpResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gemini ParseWorkerFilter error: {ex.Message}");
                return new WorkerFilterNlpResult();
            }
        }

        // =====================================================
        // HTTP
        // =====================================================
        private async Task<string> Send(object body, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl();

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
                return json;

            if ((int)response.StatusCode == 429)
                throw new BadRequestException("Gemini rate limit");

            throw new BadRequestException(
                $"Gemini error {(int)response.StatusCode}: {json}");
        }

        // =====================================================
        // LOCAL NLP
        // =====================================================

        private static readonly Dictionary<string, string[]> SkillAliases = new()
        {
            ["electrical"] = new[]
            {
                "thợ điện",
                "điện",
                "dien",
                "electric",
                "electrician"
            },

            ["plumbing"] = new[]
            {
                "ống nước",
                "thợ nước",
                "nuoc",
                "plumber",
                "plumbing"
            },

            ["welding"] = new[]
            {
                "hàn",
                "han",
                "welder",
                "welding"
            },

            ["cleaning"] = new[]
            {
                "vệ sinh",
                "ve sinh",
                "lau dọn",
                "cleaner",
                "janitor"
            },

            ["hvac"] = new[]
            {
                "máy lạnh",
                "dieu hoa",
                "điều hòa",
                "air conditioner",
                "hvac"
            }
        };

        private bool IsEmpty(WorkerFilterNlpResult r)
        {
            return string.IsNullOrWhiteSpace(r.Address)
                   && !r.SkillCategories.Any()
                   && !r.CertificateCategories.Any()
                   && !r.StartAt.HasValue
                   && !r.EndAt.HasValue
                   && !r.IsAvailable.HasValue;
        }

        // =====================================================
        // PROMPT
        // =====================================================

        private string BuildPrompt(string query)
        {
            return $$"""
You are an AI system that extracts worker search filters.

Understand Vietnamese and English.

Infer:
- worker profession
- skills
- certifications
- locations
- availability
- date ranges

Examples:

"Tìm thợ điện ở quận 1"
=> electrical skill + district 1

"Cần người biết hàn"
=> welding skill

"Worker có chứng chỉ an toàn lao động"
=> safety certification

Return ONLY valid JSON.

INPUT:
{{query}}

OUTPUT:
{
  "address": "",
  "skillCategories": [],
  "certificateCategories": [],
  "startAt": null,
  "endAt": null,
  "isAvailable": true
}
""";
        }

        // =====================================================
        // EXTRACT GEMINI TEXT
        // =====================================================

        private string ExtractText(string json)
        {
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("candidates", out var c)
                || c.GetArrayLength() == 0)
            {
                throw new Exception("Invalid Gemini response");
            }

            return c[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";
        }

        // =====================================================
        // CLEAN JSON
        // =====================================================

        private string CleanJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "{}";

            var cleaned = raw
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var start = cleaned.IndexOf('{');
            var end = cleaned.LastIndexOf('}');

            if (start >= 0 && end > start)
            {
                return cleaned.Substring(
                    start,
                    end - start + 1);
            }

            return "{}";
        }

        // =====================================================
        // TEXT NORMALIZE
        // =====================================================

        private static string NormalizeSearchText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var formD = value
                .Trim()
                .ToLowerInvariant()
                .Normalize(NormalizationForm.FormD);

            var builder = new StringBuilder(formD.Length);

            foreach (var ch in formD)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);

                if (category != UnicodeCategory.NonSpacingMark)
                    builder.Append(ch == 'đ' ? 'd' : ch);
            }

            return Regex.Replace(
                    builder.ToString().Normalize(NormalizationForm.FormC),
                    @"\s+",
                    " ")
                .Trim();
        }
    }
}