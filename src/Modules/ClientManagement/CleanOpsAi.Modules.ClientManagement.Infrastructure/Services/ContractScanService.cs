using Azure.AI.DocumentIntelligence;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.ContractScan;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Services
{
    public class ContractScanService : IContractScanService
    {
        private readonly IContractRepository _contractRepository;
        private readonly DocumentIntelligenceClient _documentIntelligenceClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ContractScanService> _logger;
        private readonly HttpClient _httpClient;

        public ContractScanService(
            IContractRepository contractRepository,
            DocumentIntelligenceClient documentIntelligenceClient,
            IConfiguration configuration,
            ILogger<ContractScanService> logger,
            HttpClient httpClient)
        {
            _contractRepository = contractRepository;
            _documentIntelligenceClient = documentIntelligenceClient;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<ContractScanResult> ScanContractAsync(Guid contractId, CancellationToken cancellationToken = default)
        {
            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null) 
                throw new NotFoundException(nameof(Contract), contractId); 

            if (string.IsNullOrWhiteSpace(contract.UrlFile))
            {
                throw new InvalidOperationException("Contract does not have an associated file URL to scan.");
            }

            // 1. Extract raw text from the document via Azure Document Intelligence
            var extractedText = await ExtractTextFromDocumentAsync(contract.UrlFile, cancellationToken);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                return new ContractScanResult
                {
                    ContractId = contract.Id,
                    ContractName = contract.Name,
                    Warnings = ["No text could be extracted from the document."]
                };
            }

            // 2. Pass extracted text to Gemini for structured parsing
            var structuredData = await ParseWithGeminiAsync(extractedText, cancellationToken);

            return new ContractScanResult
            {
                ContractId = contract.Id,
                ContractName = contract.Name,
                ExtractedRawText = extractedText,
                Slas = structuredData?.Slas ?? [],
                Warnings = structuredData?.Warnings ?? []
            };
        }

        private async Task<string> ExtractTextFromDocumentAsync(string fileUrl, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting Document Intelligence extraction for URL: {Url}", fileUrl);

                var options = new AnalyzeDocumentOptions("prebuilt-read", new Uri(fileUrl));
                var operation = await _documentIntelligenceClient.AnalyzeDocumentAsync(
                    Azure.WaitUntil.Completed,
                    options,
                    cancellationToken: cancellationToken);

                var result = operation.Value;
                return result.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract text using Azure Document Intelligence.");
                throw new Exception("Failed to analyze the contract document.", ex);
            }
        }

        private async Task<GeminiResponseFormat?> ParseWithGeminiAsync(string text, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sending extracted text to Gemini for structuring.");

                var apiKey = _configuration["Gemini:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new InvalidOperationException("Gemini API key is not configured.");
                }

                var prompt = ContractScanPromptBuilder.BuildUserPrompt(text);

                // Use the gemini-1.5-flash model, which is good for text extraction and JSON output
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

                var requestPayload = new
                {
                    contents = new[]
                    {
                        new {
                            parts = new[] {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        responseMimeType = "application/json",
                        temperature = 0.1
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(url, requestPayload, cancellationToken);
                response.EnsureSuccessStatusCode();

                var geminiResult = await response.Content.ReadFromJsonAsync<GeminiApiResponse>(cancellationToken: cancellationToken);
                var jsonText = geminiResult?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrWhiteSpace(jsonText))
                {
                    _logger.LogWarning("Gemini returned empty text.");
                    return null;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };

                var structuredResponse = JsonSerializer.Deserialize<GeminiResponseFormat>(jsonText, options);
                return structuredResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse text with Gemini.");
                throw new Exception("Failed to parse the contract structure using AI.", ex);
            }
        }

        // Internal classes to handle Gemini's HTTP response shape
        private class GeminiApiResponse
        {
            public List<GeminiCandidate>? Candidates { get; set; }
        }

        private class GeminiCandidate
        {
            public GeminiContent? Content { get; set; }
        }

        private class GeminiContent
        {
            public List<GeminiPart>? Parts { get; set; }
        }

        private class GeminiPart
        {
            public string? Text { get; set; }
        }

        // Schema expected from the prompt
        private class GeminiResponseFormat
        {
            public List<ExtractedSlaDto> Slas { get; set; } = [];
            public List<string> Warnings { get; set; } = [];
        }
    }
}
