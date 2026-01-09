using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIKeyMouse.CodeGen.CLI.Models.Configuration;
using AIKeyMouse.CodeGen.CLI.Models.LLM;
using Microsoft.Extensions.Logging;

namespace AIKeyMouse.CodeGen.CLI.Services.LLM;

/// <summary>
/// Groq API provider implementation
/// https://console.groq.com/docs/api-reference
/// </summary>
public class GroqProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly GroqConfig _config;
    private readonly ILogger<GroqProvider> _logger;

    public string Name => "groq";
    public int Priority => _config.Priority;
    public bool IsAvailable => !string.IsNullOrWhiteSpace(_config.ApiKey);

    public GroqProvider(
        HttpClient httpClient,
        GroqConfig config,
        ILogger<GroqProvider> logger)
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
        
        _logger.LogDebug("Sending request to Groq API with model {Model}", model);

        // Build messages array
        var messages = new List<object>();
        
        if (!string.IsNullOrWhiteSpace(request.SystemMessage))
        {
            messages.Add(new { role = "system", content = request.SystemMessage });
        }

        // Add history if present
        if (request.History != null)
        {
            foreach (var msg in request.History)
            {
                messages.Add(new { role = msg.Role, content = msg.Content });
            }
        }

        // Add current prompt
        messages.Add(new { role = "user", content = request.Prompt });

        var requestBody = new
        {
            model,
            messages,
            temperature = request.Temperature,
            max_tokens = request.MaxTokens,
            top_p = 1,
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync(
            "chat/completions",
            requestBody,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GroqApiResponse>(cancellationToken);
        
        if (result?.Choices == null || result.Choices.Count == 0)
        {
            throw new InvalidOperationException("Groq API returned no choices");
        }

        var choice = result.Choices[0];
        
        _logger.LogInformation(
            "Groq API completed: {Tokens} tokens ({Prompt} prompt + {Completion} completion)",
            result.Usage?.TotalTokens ?? 0,
            result.Usage?.PromptTokens ?? 0,
            result.Usage?.CompletionTokens ?? 0);

        return new LlmResponse
        {
            Content = choice.Message.Content,
            Model = result.Model,
            Provider = Name,
            PromptTokens = result.Usage?.PromptTokens ?? 0,
            CompletionTokens = result.Usage?.CompletionTokens ?? 0,
            FinishReason = choice.FinishReason
        };
    }

    public (bool IsValid, string? ErrorMessage) Validate()
    {
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            return (false, "Groq API key is not configured. Set GROQ_API_KEY environment variable.");
        }

        if (string.IsNullOrWhiteSpace(_config.BaseUrl))
        {
            return (false, "Groq base URL is not configured");
        }

        return (true, null);
    }

    #region API Response Models

    private class GroqApiResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("choices")]
        public List<GroqChoice> Choices { get; set; } = new();

        [JsonPropertyName("usage")]
        public GroqUsage? Usage { get; set; }
    }

    private class GroqChoice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public GroqMessage Message { get; set; } = new();

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; } = string.Empty;
    }

    private class GroqMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private class GroqUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    #endregion
}
