using CleanOpsAi.Modules.Workforce.Application.Dtos.Nlps;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CleanOpsAi.Modules.Workforce.Application.Services
{
    public static class WorkerNlpLocalParser
    {
        private static readonly Dictionary<string, string[]> SkillAliases = new()
        {
            ["cleaning"] =
            [
                "don dep", "don ve sinh", "ve sinh", "lau don", "tap vu",
                "cleaning", "cleaner", "janitor", "don dep co ban", "ve sinh co ban"
            ],
            ["electrical"] =
            [
                "tho dien", "dien", "sua dien", "electric", "electrician"
            ],
            ["plumbing"] =
            [
                "ong nuoc", "tho nuoc", "sua nuoc", "plumber", "plumbing"
            ],
            ["welding"] =
            [
                "han", "tho han", "welder", "welding"
            ],
            ["hvac"] =
            [
                "may lanh", "dieu hoa", "air conditioner", "hvac"
            ],
            ["working at height"] =
            [
                "lam viec tren cao", "lam tren cao", "tren cao",
                "working at height", "work at height", "height safety"
            ]
        };

        private static readonly Dictionary<string, string[]> CertificationAliases = new()
        {
            ["safety"] =
            [
                "an toan lao dong", "bao ho lao dong", "safety", "osh", "hse"
            ],
            ["fire safety"] =
            [
                "pccc", "phong chay chua chay", "chua chay", "fire safety"
            ],
            ["first aid"] =
            [
                "so cuu", "cap cuu", "first aid"
            ],
            ["chemical"] =
            [
                "hoa chat", "chemical"
            ],
            ["working at height"] =
            [
                "chung chi lam viec tren cao", "an toan lam viec tren cao",
                "working at height", "height safety"
            ]
        };

        private static readonly string[] SkillLeadWords =
        [
            "skill", "skills", "ky nang", "ki nang", "biet", "lam duoc", "lam tot", "lam", "su dung"
        ];

        private static readonly string[] CertificateLeadWords =
        [
            "certificate", "cert", "chung chi", "chung nhan", "bang"
        ];

        public static WorkerFilterNlpResult Parse(string? query)
        {
            var result = new WorkerFilterNlpResult
            {
                SkillCategories = new List<string>(),
                CertificateCategories = new List<string>()
            };

            if (string.IsNullOrWhiteSpace(query))
                return result;

            var normalized = NormalizeSearchText(query);
            var hasSkillLeadWord = ContainsAnyLeadWord(normalized, SkillLeadWords);
            var hasCertificateLeadWord = ContainsAnyLeadWord(normalized, CertificateLeadWords);

            if (!hasCertificateLeadWord || hasSkillLeadWord)
                InferAliases(normalized, SkillAliases, result.SkillCategories);

            InferAliases(normalized, CertificationAliases, result.CertificateCategories, requireLeadWord: true);

            ExtractTerms(normalized, result.CertificateCategories, CertificateLeadWords);
            ExtractTerms(normalized, result.SkillCategories, SkillLeadWords);
            ExtractBareHasSkillTerms(normalized, result.SkillCategories);

            result.Address = ExtractAddress(normalized);

            if (Regex.IsMatch(normalized, @"\b(ranh|khong ban|available|con lich)\b"))
                result.IsAvailable = true;

            ExtractDateRange(normalized, result);
            Normalize(result);

            return result;
        }

        public static WorkerFilterNlpResult Merge(WorkerFilterNlpResult preferred, WorkerFilterNlpResult fallback)
        {
            preferred.Address = string.IsNullOrWhiteSpace(preferred.Address)
                ? fallback.Address
                : preferred.Address;

            preferred.StartAt ??= fallback.StartAt;
            preferred.EndAt ??= fallback.EndAt;
            preferred.IsAvailable ??= fallback.IsAvailable;

            preferred.SkillCategories = (preferred.SkillCategories ?? new List<string>())
                .Union(fallback.SkillCategories ?? new List<string>(), StringComparer.OrdinalIgnoreCase)
                .ToList();

            preferred.CertificateCategories = (preferred.CertificateCategories ?? new List<string>())
                .Union(fallback.CertificateCategories ?? new List<string>(), StringComparer.OrdinalIgnoreCase)
                .ToList();

            Normalize(preferred);
            return preferred;
        }

        public static string NormalizeSearchText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var formD = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(formD.Length);

            foreach (var ch in formD)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category != UnicodeCategory.NonSpacingMark)
                    builder.Append(ch == 'đ' ? 'd' : ch);
            }

            return Regex.Replace(builder.ToString().Normalize(NormalizationForm.FormC), @"\s+", " ").Trim();
        }

        private static void InferAliases(
            string normalizedQuery,
            Dictionary<string, string[]> aliases,
            List<string> target,
            bool requireLeadWord = false)
        {
            if (requireLeadWord && !ContainsAnyLeadWord(normalizedQuery, CertificateLeadWords))
                return;

            foreach (var aliasGroup in aliases)
            {
                if (aliasGroup.Value.Any(alias => ContainsPhrase(normalizedQuery, alias)))
                    target.Add(aliasGroup.Key);
            }
        }

        private static void ExtractTerms(string input, List<string> target, string[] leadWords)
        {
            var leadPattern = string.Join("|", leadWords.Select(Regex.Escape));
            var stopPattern = BuildStopPattern();
            var pattern = $@"(?:\bco\s+)?(?:{leadPattern})\s+(.+?)(?=\s+(?:{stopPattern})\b|$)";

            foreach (Match match in Regex.Matches(input, pattern, RegexOptions.IgnoreCase))
            {
                foreach (var term in Regex.Split(match.Groups[1].Value, @"\s*(?:,|;|/|\+|\bva\b|\band\b)\s*", RegexOptions.IgnoreCase))
                {
                    var cleaned = Regex.Replace(
                        term,
                        @"\b(co|can|voi|ve|o|tai)\b",
                        " ",
                        RegexOptions.IgnoreCase).Trim();

                    if (!string.IsNullOrWhiteSpace(cleaned))
                        target.Add(cleaned);
                }
            }
        }

        private static void ExtractBareHasSkillTerms(string input, List<string> target)
        {
            var stopPattern = BuildStopPattern();
            var pattern = $@"\bco\s+(?!(?:{BuildCertificateLeadPattern()})\b)(.+?)(?=\s+(?:{stopPattern})\b|$)";

            foreach (Match match in Regex.Matches(input, pattern, RegexOptions.IgnoreCase))
            {
                var cleaned = Regex.Replace(
                    match.Groups[1].Value,
                    @"\b(can|nguoi|nhan vien|worker|voi|ve|o|tai)\b",
                    " ",
                    RegexOptions.IgnoreCase).Trim();

                if (!string.IsNullOrWhiteSpace(cleaned))
                    target.Add(cleaned);
            }
        }

        private static string? ExtractAddress(string normalizedQuery)
        {
            var markerPattern = @"(?<!\w)(?:o|tai|in|gan|khu vuc)\s+";
            var match = Regex.Match(normalizedQuery, markerPattern, RegexOptions.IgnoreCase);

            if (!match.Success)
                return null;

            var address = normalizedQuery[(match.Index + match.Length)..].Trim();
            address = CutAtNextFilterPhrase(address);
            address = RemoveTrailingKnownPhrases(address);
            address = Regex.Replace(address, @"\b(co|can|nguoi|nhan vien|worker|tim)\b", " ", RegexOptions.IgnoreCase);
            address = Regex.Replace(address, @"\s+", " ").Trim();

            return string.IsNullOrWhiteSpace(address) ? null : address;
        }

        private static string CutAtNextFilterPhrase(string value)
        {
            var stopPattern = BuildStopPattern();
            var match = Regex.Match(value, $@"\s+(?:{stopPattern})\b", RegexOptions.IgnoreCase);
            return match.Success ? value[..match.Index].Trim() : value;
        }

        private static string RemoveTrailingKnownPhrases(string value)
        {
            var result = value;
            var knownPhrases = SkillAliases.Values
                .Concat(CertificationAliases.Values)
                .SelectMany(x => x)
                .Select(NormalizeSearchText)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .OrderByDescending(x => x.Length)
                .ToList();

            foreach (var phrase in knownPhrases)
            {
                var index = result.IndexOf(phrase, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                    result = result[..index].Trim();
            }

            return result;
        }

        private static string BuildStopPattern()
        {
            var stopWords = SkillLeadWords
                .Concat(CertificateLeadWords)
                .Concat(["co", "o", "tai", "in", "gan", "khu vuc", "khong ban", "ranh", "tu", "from"]);

            var aliasWords = SkillAliases.Values
                .Concat(CertificationAliases.Values)
                .SelectMany(x => x);

            return string.Join("|", stopWords
                .Concat(aliasWords)
                .Select(NormalizeSearchText)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(x => x.Length)
                .Select(Regex.Escape));
        }

        private static string BuildCertificateLeadPattern()
        {
            return string.Join("|", CertificateLeadWords.Select(Regex.Escape));
        }

        private static bool ContainsAnyLeadWord(string input, string[] leadWords)
        {
            return leadWords.Any(word => Regex.IsMatch(input, $@"\b{Regex.Escape(word)}\b", RegexOptions.IgnoreCase));
        }

        private static bool ContainsPhrase(string input, string phrase)
        {
            var normalizedPhrase = NormalizeSearchText(phrase);
            return !string.IsNullOrWhiteSpace(normalizedPhrase)
                   && Regex.IsMatch(input, $@"(?<!\w){Regex.Escape(normalizedPhrase)}(?!\w)", RegexOptions.IgnoreCase);
        }

        private static void ExtractDateRange(string input, WorkerFilterNlpResult result)
        {
            var range = Regex.Match(
                input,
                @"(?:tu|from)\s+(.+?)\s+(?:den|to|-)\s+(.+?)(?=$|\s+(?:o|tai|skill|certificate|chung chi)\b)",
                RegexOptions.IgnoreCase);

            if (!range.Success)
                return;

            if (TryParseNaturalDate(range.Groups[1].Value, out var startAt))
                result.StartAt = startAt;

            if (TryParseNaturalDate(range.Groups[2].Value, out var endAt))
                result.EndAt = endAt.Date.AddDays(1).AddTicks(-1);
        }

        private static bool TryParseNaturalDate(string input, out DateTime value)
        {
            input = input.Trim();

            var formats = new[]
            {
                "d/M/yyyy", "dd/MM/yyyy", "d-M-yyyy", "dd-MM-yyyy",
                "d/M", "dd/MM", "d-M", "dd-MM", "yyyy-MM-dd"
            };

            if (DateTime.TryParseExact(
                input,
                formats,
                CultureInfo.GetCultureInfo("vi-VN"),
                DateTimeStyles.AssumeLocal,
                out value))
            {
                if (value.Year == 1)
                    value = new DateTime(DateTime.UtcNow.Year, value.Month, value.Day);

                return true;
            }

            return DateTime.TryParse(
                input,
                CultureInfo.GetCultureInfo("vi-VN"),
                DateTimeStyles.AssumeLocal,
                out value);
        }

        private static void Normalize(WorkerFilterNlpResult result)
        {
            result.SkillCategories = result.SkillCategories?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            result.CertificateCategories = result.CertificateCategories?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            if (!string.IsNullOrWhiteSpace(result.Address))
                result.Address = result.Address.Trim();
        }
    }
}
