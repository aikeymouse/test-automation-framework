using AIKeyMouse.CodeGen.CLI.Models.Configuration;
using ILogger = Serilog.ILogger;

namespace AIKeyMouse.CodeGen.CLI.Services.Infrastructure;

/// <summary>
/// Service for configuring and managing logging
/// </summary>
public static class LoggingService
{
    /// <summary>
    /// Configure Serilog logger based on configuration
    /// </summary>
    public static ILogger ConfigureLogger(LoggingConfiguration config)
    {
        var loggerConfig = new LoggerConfiguration();

        // Set minimum level
        var minimumLevel = ParseLogLevel(config.MinimumLevel);
        loggerConfig.MinimumLevel.Is(minimumLevel);

        // Console logging
        if (config.EnableConsoleLogging)
        {
            loggerConfig.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
        }

        // File logging
        if (config.EnableFileLogging)
        {
            var rollingInterval = ParseRollingInterval(config.RollingInterval);
            
            loggerConfig.WriteTo.File(
                path: config.LogFilePath,
                rollingInterval: rollingInterval,
                retainedFileCountLimit: config.RetainedFileCountLimit,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
        }

        // Enrich with additional information
        loggerConfig.Enrich.FromLogContext();

        return loggerConfig.CreateLogger();
    }

    /// <summary>
    /// Parse log level string to Serilog event level
    /// </summary>
    private static Serilog.Events.LogEventLevel ParseLogLevel(string level)
    {
        return level.ToLowerInvariant() switch
        {
            "verbose" => Serilog.Events.LogEventLevel.Verbose,
            "debug" => Serilog.Events.LogEventLevel.Debug,
            "information" => Serilog.Events.LogEventLevel.Information,
            "warning" => Serilog.Events.LogEventLevel.Warning,
            "error" => Serilog.Events.LogEventLevel.Error,
            "fatal" => Serilog.Events.LogEventLevel.Fatal,
            _ => Serilog.Events.LogEventLevel.Information
        };
    }

    /// <summary>
    /// Parse rolling interval string
    /// </summary>
    private static Serilog.RollingInterval ParseRollingInterval(string interval)
    {
        return interval.ToLowerInvariant() switch
        {
            "infinite" => Serilog.RollingInterval.Infinite,
            "year" => Serilog.RollingInterval.Year,
            "month" => Serilog.RollingInterval.Month,
            "day" => Serilog.RollingInterval.Day,
            "hour" => Serilog.RollingInterval.Hour,
            "minute" => Serilog.RollingInterval.Minute,
            _ => Serilog.RollingInterval.Day
        };
    }
}
