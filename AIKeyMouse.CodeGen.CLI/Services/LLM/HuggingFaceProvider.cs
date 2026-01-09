using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AIKeyMouse.CodeGen.CLI.Models.Configuration;
using AIKeyMouse.CodeGen.CLI.Models.LLM;
using Microsoft.Extensions.Logging;

namespace AIKeyMouse.CodeGen.CLI.Services.LLM;

/// <summary>
/// HuggingFace Inference API provider implementation
/// https://huggingface.co/docs/api-inference/
/// </summary>
public class HuggingFaceProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly HuggingFaceConfig _config;
    private readonly ILogger<HuggingFaceProvider> _logger;

    public string Name => "huggingface";
    public int Priority => _config.Priority;
    public bool IsAvailable => !string.IsNullOrWhiteSpace(_config.ApiKey);

    public HuggingFaceProvider(
        HttpClient httpClient,
        HuggingFaceConfig config,
        ILogger<HuggingFaceProvider> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;

        // Configure HTTP client
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
    }

    public async Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var model = request.Model ?? _config.DefaultModel;
        
        _logger.LogDebug("Sending request to HuggingFace API with model {Model}", model);

        // Build the full prompt
        var fullPrompt = BuildPrompt(request);

        var requestBody = new
        {
            inputs = fullPrompt,
            parameters = new
            {
                max_new_tokens = request.MaxTokens,
                temperature = request.Temperature,
                top_p = 0.95,
                return_full_text = false
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"models/{model}",
            requestBody,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<HuggingFaceResponse>>(cancellationToken);
        
        if (result == null || result.Count == 0)
        {
            throw new InvalidOperationException("HuggingFace API returned no results");
        }

        var generatedText = result[0].GeneratedText;
        
        _logger.LogInformation("HuggingFace API completed with model {Model}", model);

        return new LlmResponse
        {
            Content = generatedText,
            Model = model,
            Provider = Name,
            PromptTokens = EstimateTokens(fullPrompt),
            CompletionTokens = EstimateTokens(generatedText),
            FinishReason = "stop"
        };
    }

    public (bool IsValid, string? ErrorMessage) Validate()
    {
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            return (false, "HuggingFace API key is not configured. Set HUGGINGFACE_API_KEY environment variable.");
        }

        if (string.IsNullOrWhiteSpace(_config.BaseUrl))
        {
            return (false, "HuggingFace base URL is not configured");
        }

        return (true, null);
    }

    private string BuildPrompt(LlmRequest request)
    {
        var parts = new List<string>();

        // Add system message if present
        if (!string.IsNullOrWhiteSpace(request.SystemMessage))
        {
            parts.Add($"<|system|>\n{request.SystemMessage}");
        }

        // Add history if present
        if (request.History != null)
        {
            foreach (var msg in request.History)
            {
                var tag = msg.Role == "user" ? "user" : "assistant";
                parts.Add($"<|{tag}|>\n{msg.Content}");
            }
        }

        // Add current prompt
        parts.Add($"<|user|>\n{request.Prompt}");
        parts.Add("<|assistant|>");

        return string.Join("\n", parts);
    }

    private int EstimateTokens(string text)
    {
        // Rough estimation: ~4 characters per token
        return text.Length / 4;
    }

    #region API Response Models

    private class HuggingFaceResponse
    {
        [JsonPropertyName("generated_text")]
        public string GeneratedText { get; set; } = string.Empty;
    }

    #endregion
}
