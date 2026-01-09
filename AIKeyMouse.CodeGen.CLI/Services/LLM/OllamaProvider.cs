using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIKeyMouse.CodeGen.CLI.Models.Configuration;
using AIKeyMouse.CodeGen.CLI.Models.LLM;
using Microsoft.Extensions.Logging;

namespace AIKeyMouse.CodeGen.CLI.Services.LLM;

/// <summary>
/// Ollama local LLM provider implementation
/// https://github.com/ollama/ollama/blob/main/docs/api.md
/// </summary>
public class OllamaProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly OllamaConfig _config;
    private readonly ILogger<OllamaProvider> _logger;
    private bool? _isAvailable;
    private DateTime _lastCheck = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public string Name => "ollama";
    public int Priority => _config.Priority;
    
    public bool IsAvailable
    {
        get
        {
            // Cache the availability check for 30 seconds to avoid repeated HTTP calls
            if (_isAvailable.HasValue && DateTime.UtcNow - _lastCheck < CacheDuration)
            {
                return _isAvailable.Value;
            }

            _isAvailable = CheckAvailability();
            _lastCheck = DateTime.UtcNow;
            return _isAvailable.Value;
        }
    }

    public OllamaProvider(
        HttpClient httpClient,
        OllamaConfig config,
        ILogger<OllamaProvider> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;

        // Configure HTTP client
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        // Set timeout to prevent premature cancellation (must be >= provider timeout)
        _httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(_config.TimeoutSeconds + 10, 310));
        _logger.LogInformation("OllamaProvider initialized - HttpClient.Timeout: {Timeout}s, Config.TimeoutSeconds: {ConfigTimeout}s", 
            _httpClient.Timeout.TotalSeconds, _config.TimeoutSeconds);
    }

    private bool CheckAvailability()
    {
        try
        {
            // Quick health check to Ollama API with 2 second timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var response = _httpClient.GetAsync("/api/tags", cts.Token).Result;
            var available = response.IsSuccessStatusCode;
            
            if (available)
            {
                _logger.LogDebug("Ollama is available at {BaseUrl}", _config.BaseUrl);
            }
            else
            {
                _logger.LogDebug("Ollama returned status {Status}", response.StatusCode);
            }
            
            return available;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Ollama is not available at {BaseUrl}", _config.BaseUrl);
            return false;
        }
    }

    public async Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var model = request.Model ?? _config.DefaultModel;
        
        _logger.LogInformation("Using Ollama model: {Model} (Temperature: {Temperature}, MaxTokens: {MaxTokens})", 
            model, request.Temperature, request.MaxTokens);

        // Use configured timeout for code generation
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_config.TimeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

        // Build prompt with context
        var prompt = request.Prompt;
        
        if (!string.IsNullOrWhiteSpace(request.SystemMessage))
        {
            prompt = $"{request.SystemMessage}\n\n{prompt}";
        }

        // Add history if present
        if (request.History != null && request.History.Count > 0)
        {
            var historyText = string.Join("\n\n", request.History.Select(h => 
                $"{h.Role}: {h.Content}"));
            prompt = $"{historyText}\n\n{prompt}";
        }

        var requestBody = new
        {
            model = model,
            prompt = prompt,
            stream = false,
            options = new
            {
                temperature = request.Temperature,
                num_predict = request.MaxTokens
            }
        };

        _logger.LogDebug("==== OLLAMA REQUEST ====");
        _logger.LogDebug("Model: {Model}", model);
        _logger.LogDebug("System Message: {SystemMessage}", request.SystemMessage ?? "(none)");
        _logger.LogDebug("User Prompt: {Prompt}", request.Prompt);
        _logger.LogDebug("========================");

        var response = await _httpClient.PostAsJsonAsync(
            "/api/generate",
            requestBody,
            linkedCts.Token);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(
            cancellationToken: linkedCts.Token);

        if (result == null)
        {
            throw new InvalidOperationException("Failed to deserialize Ollama response");
        }

        _logger.LogDebug("==== OLLAMA RESPONSE ====");
        _logger.LogDebug("Tokens: Prompt={PromptTokens}, Completion={CompletionTokens}, Total={TotalTokens}",
            result.PromptEvalCount, result.EvalCount, result.PromptEvalCount + result.EvalCount);
        _logger.LogDebug("Response length: {Length} characters", result.Response.Length);
        _logger.LogDebug("Response content: {Content}", result.Response);
        _logger.LogDebug("=========================");

        return new LlmResponse
        {
            Content = result.Response,
            Model = model,
            Provider = Name,
            PromptTokens = result.PromptEvalCount,
            CompletionTokens = result.EvalCount
        };
    }

    public (bool IsValid, string? ErrorMessage) Validate()
    {
        if (!IsAvailable)
        {
            return (false, $"Ollama is not running at {_config.BaseUrl}. Start Ollama with 'ollama serve' or install from https://ollama.ai");
        }

        if (string.IsNullOrWhiteSpace(_config.DefaultModel))
        {
            return (false, "Default model is not configured");
        }

        return (true, null);
    }

    private class OllamaResponse
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

        [JsonPropertyName("done")]
        public bool Done { get; set; }

        [JsonPropertyName("total_duration")]
        public long TotalDuration { get; set; }

        [JsonPropertyName("load_duration")]
        public long LoadDuration { get; set; }

        [JsonPropertyName("prompt_eval_count")]
        public int PromptEvalCount { get; set; }

        [JsonPropertyName("eval_count")]
        public int EvalCount { get; set; }

        [JsonPropertyName("eval_duration")]
        public long EvalDuration { get; set; }
    }
}
