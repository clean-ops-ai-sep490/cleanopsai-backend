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

            // timeout ngắn để tránh treo request
            _httpClient.Timeout = TimeSpan.FromSeconds(2);
        }

        private string BuildUrl()
        {
            return $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";
        }

        // ========================= CHAT =========================
        public async Task<string> ChatAsync(string message)
        {
            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = message?.Trim() ?? "" } }
                    }
                }
            };

            var json = await Send(body);
            return ExtractText(json);
        }

        // ========================= NLP =========================
        public async Task<WorkerFilterNlpResult> ParseWorkerFilterAsync(string query)
        {
            query = query?.Trim() ?? "";

            // 🔥 LOCAL FIRST (luôn chạy nhanh)
            var local = LocalParse(query);

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
                    parts = new[] { new { text = prompt } }
                }
            }
                };

                var json = await Send(body);
                var raw = ExtractText(json);
                var cleaned = CleanJson(raw);

                var result = JsonSerializer.Deserialize<WorkerFilterNlpResult>(
                    cleaned,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

                if (result == null)
                    return local;

                Normalize(result);

                return IsEmpty(result) ? local : result;
            }
            catch (Exception ex)
            {
                // 🔥 QUAN TRỌNG: FAIL FAST → KHÔNG THROW RA PIPELINE
                Console.WriteLine($"Gemini fail: {ex.Message}");
                return local;
            }
        }

        // ========================= GEMINI HTTP (NO RETRY) =========================
        private async Task<string> Send(object body)
        {
            var url = BuildUrl();

            var requestContent = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, requestContent);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return json;

            // ❌ KHÔNG retry để tránh treo system
            if ((int)response.StatusCode == 429)
                throw new BadRequestException("Gemini rate limit (429)");

            throw new BadRequestException($"Gemini error: {(int)response.StatusCode} - {json}");
        }

        // ========================= LOCAL PARSE =========================
        private WorkerFilterNlpResult LocalParse(string query)
        {
            var result = new WorkerFilterNlpResult
            {
                Address = null,
                SkillCategories = new List<string>(),
                CertificateCategories = new List<string>()
            };

            if (string.IsNullOrWhiteSpace(query))
                return result;

            var lower = query.ToLower();

            // skill
            var skill = System.Text.RegularExpressions.Regex.Match(lower, @"skill\s+(.+)");
            if (skill.Success)
                result.SkillCategories.Add(skill.Groups[1].Value.Trim());

            // cert
            var cert = System.Text.RegularExpressions.Regex.Match(lower, @"(certificate|cert|chứng chỉ)\s+(.+)");
            if (cert.Success)
                result.CertificateCategories.Add(cert.Groups[2].Value.Trim());

            // address fix
            var addr = System.Text.RegularExpressions.Regex.Match(lower, @"(ở|tại|in)\s+(.+)");
            if (addr.Success)
            {
                var value = addr.Groups[2].Value.Trim();

                value = System.Text.RegularExpressions.Regex.Replace(value, @"skill.*", "").Trim();
                value = System.Text.RegularExpressions.Regex.Replace(value, @"certificate.*", "").Trim();
                value = System.Text.RegularExpressions.Regex.Replace(value, @"\s+", " ").Trim();

                if (value.Length > 2)
                    result.Address = value;
            }

            return result;
        }

        // ========================= NORMALIZE =========================
        private void Normalize(WorkerFilterNlpResult r)
        {
            r.SkillCategories = r.SkillCategories?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower())
                .Distinct()
                .ToList() ?? new();

            r.CertificateCategories = r.CertificateCategories?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLower())
                .Distinct()
                .ToList() ?? new();

            if (!string.IsNullOrWhiteSpace(r.Address))
                r.Address = r.Address.Trim();
        }

        private bool IsEmpty(WorkerFilterNlpResult r)
        {
            return string.IsNullOrWhiteSpace(r.Address)
                && !r.SkillCategories.Any()
                && !r.CertificateCategories.Any();
        }

        // ========================= PROMPT =========================
        private string BuildPrompt(string query)
        {
            return $@"
Extract worker search filters.

INPUT:
{query}

OUTPUT JSON ONLY:
{{
  ""address"": """",
  ""skillCategories"": [],
  ""certificateCategories"": []
}}";
        }

        // ========================= EXTRACT =========================
        private string ExtractText(string json)
        {
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("candidates", out var c) || c.GetArrayLength() == 0)
                throw new Exception("Invalid Gemini response");

            return c[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";
        }

        // ========================= CLEAN JSON =========================
        private string CleanJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "{}";

            var cleaned = raw.Replace("```json", "").Replace("```", "").Trim();

            var start = cleaned.IndexOf('{');
            var end = cleaned.LastIndexOf('}');

            if (start >= 0 && end > start)
                return cleaned.Substring(start, end - start + 1);

            return "{}";
        }
    }
}