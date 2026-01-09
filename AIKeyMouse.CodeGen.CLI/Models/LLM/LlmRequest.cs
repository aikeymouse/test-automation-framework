namespace AIKeyMouse.CodeGen.CLI.Models.LLM;

/// <summary>
/// Request to an LLM provider
/// </summary>
public class LlmRequest
{
    /// <summary>
    /// The prompt/user message to send
    /// </summary>
    public required string Prompt { get; set; }

    /// <summary>
    /// System message for context/instructions
    /// </summary>
    public string? SystemMessage { get; set; }

    /// <summary>
    /// Temperature (0.0-1.0) - controls randomness
    /// </summary>
    public float Temperature { get; set; } = 0.5f;

    /// <summary>
    /// Maximum tokens to generate
    /// </summary>
    public int MaxTokens { get; set; } = 2500;

    /// <summary>
    /// Model to use (provider-specific)
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Optional conversation history
    /// </summary>
    public List<LlmMessage>? History { get; set; }
}

/// <summary>
/// A single message in conversation history
/// </summary>
public class LlmMessage
{
    public required string Role { get; set; } // "system", "user", "assistant"
    public required string Content { get; set; }
}
