using AIKeyMouse.CodeGen.CLI.Models.Configuration;
using AIKeyMouse.CodeGen.CLI.Services.Infrastructure;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AIKeyMouse.CodeGen.CLI.Commands;

/// <summary>
/// Base class for all CLI commands
/// </summary>
public abstract class BaseCommand
{
    protected readonly ILogger Logger;
    protected readonly ConfigurationService ConfigService;
    protected readonly FileService FileService;
    protected readonly CliConfig Config;

    protected BaseCommand(
        ILogger logger,
        ConfigurationService configService,
        FileService fileService)
    {
        Logger = logger;
        ConfigService = configService;
        FileService = fileService;
        Config = configService.GetConfiguration();
    }

    /// <summary>
    /// Validate command execution
    /// </summary>
    protected virtual (bool IsValid, string? ErrorMessage) ValidateExecution()
    {
        // Validate configuration
        var (isValid, errors) = ConfigService.ValidateConfiguration(Config);
        if (!isValid)
        {
            return (false, string.Join(Environment.NewLine, errors));
        }

        return (true, null);
    }

    /// <summary>
    /// Log command start
    /// </summary>
    protected void LogCommandStart(string commandName, params (string key, object? value)[] parameters)
    {
        Logger.LogInformation("Starting command: {CommandName}", commandName);
        
        foreach (var (key, value) in parameters)
        {
            Logger.LogDebug("Parameter {Key}: {Value}", key, value);
        }
    }

    /// <summary>
    /// Log command completion
    /// </summary>
    protected void LogCommandComplete(string commandName, bool success)
    {
        if (success)
        {
            Logger.LogInformation("Command completed successfully: {CommandName}", commandName);
        }
        else
        {
            Logger.LogError("Command failed: {CommandName}", commandName);
        }
    }

    /// <summary>
    /// Handle command errors
    /// </summary>
    protected int HandleError(Exception ex, string context)
    {
        Logger.LogError(ex, "Error during {Context}", context);
        Console.Error.WriteLine($"Error: {ex.Message}");
        return 1;
    }

    /// <summary>
    /// Display success message
    /// </summary>
    protected void DisplaySuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Display error message
    /// </summary>
    protected void DisplayError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✗ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Display warning message
    /// </summary>
    protected void DisplayWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Display info message
    /// </summary>
    protected void DisplayInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"ℹ {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Confirm action with user
    /// </summary>
    protected bool ConfirmAction(string message, bool defaultYes = false)
    {
        Console.Write($"{message} ({(defaultYes ? "Y/n" : "y/N")}): ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(response))
            return defaultYes;

        return response == "y" || response == "yes";
    }
}
