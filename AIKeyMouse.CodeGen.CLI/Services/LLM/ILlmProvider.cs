using AIKeyMouse.CodeGen.CLI.Models.LLM;

namespace AIKeyMouse.CodeGen.CLI.Services.LLM;

/// <summary>
/// Interface for LLM providers (Groq, HuggingFace, etc.)
/// </summary>
public interface ILlmProvider
{
    /// <summary>
    /// Provider name (e.g., "groq", "huggingface")
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Provider priority (lower = higher priority)
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Whether this provider is configured and available
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Generate completion from a prompt
    /// </summary>
    Task<LlmResponse> GenerateAsync(LlmRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate provider configuration
    /// </summary>
    (bool IsValid, string? ErrorMessage) Validate();
}
