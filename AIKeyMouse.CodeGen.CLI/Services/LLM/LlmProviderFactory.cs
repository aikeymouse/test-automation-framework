using AIKeyMouse.CodeGen.CLI.Models.Configuration;
using AIKeyMouse.CodeGen.CLI.Models.LLM;
using Microsoft.Extensions.Logging;

namespace AIKeyMouse.CodeGen.CLI.Services.LLM;

/// <summary>
/// Factory for creating and managing LLM providers with automatic failover
/// </summary>
public class LlmProviderFactory
{
    private readonly IEnumerable<ILlmProvider> _providers;
    private readonly ILogger<LlmProviderFactory> _logger;

    public LlmProviderFactory(
        IEnumerable<ILlmProvider> providers,
        ILogger<LlmProviderFactory> logger)
    {
        _providers = providers.OrderBy(p => p.Priority).ToList();
        _logger = logger;
    }

    /// <summary>
    /// Get the best available provider
    /// </summary>
    public ILlmProvider GetProvider()
    {
        var provider = _providers.FirstOrDefault(p => p.IsAvailable);
        
        if (provider == null)
        {
            throw new InvalidOperationException(
                "No LLM providers are configured. " +
                "Set GROQ_API_KEY or HUGGINGFACE_API_KEY environment variable.");
        }

        return provider;
    }

    /// <summary>
    /// Generate completion with automatic failover to next provider on error
    /// </summary>
    public async Task<LlmResponse> GenerateWithFailoverAsync(
        LlmRequest request,
        CancellationToken cancellationToken = default)
    {
        var availableProviders = _providers.Where(p => p.IsAvailable).ToList();
        
        if (availableProviders.Count == 0)
        {
            throw new InvalidOperationException(
                "No LLM providers are configured. " +
                "Set GROQ_API_KEY or HUGGINGFACE_API_KEY environment variable.");
        }

        Exception? lastException = null;

        foreach (var provider in availableProviders)
        {
            try
            {
                _logger.LogInformation("Attempting to use {Provider} provider", provider.Name);
                
                var response = await provider.GenerateAsync(request, cancellationToken);
                response.IsFallback = provider.Priority > availableProviders[0].Priority;
                
                if (response.IsFallback)
                {
                    _logger.LogWarning(
                        "Using fallback provider {Provider} (primary provider failed)", 
                        provider.Name);
                }

                return response;
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogError(
                    ex,
                    "Provider {Provider} failed, trying next provider",
                    provider.Name);
            }
        }

        throw new AggregateException(
            "All LLM providers failed",
            lastException != null ? new[] { lastException } : Array.Empty<Exception>());
    }

    /// <summary>
    /// Get all available providers
    /// </summary>
    public IEnumerable<ILlmProvider> GetAvailableProviders()
    {
        return _providers.Where(p => p.IsAvailable);
    }

    /// <summary>
    /// Validate all providers
    /// </summary>
    public Dictionary<string, (bool IsValid, string? ErrorMessage)> ValidateAll()
    {
        var results = new Dictionary<string, (bool IsValid, string? ErrorMessage)>();
        
        foreach (var provider in _providers)
        {
            results[provider.Name] = provider.Validate();
        }

        return results;
    }
}
