using Cocona;
using AIKeyMouse.CodeGen.CLI.Models.Configuration;
using AIKeyMouse.CodeGen.CLI.Services.Infrastructure;

namespace AIKeyMouse.CodeGen.CLI;

class Program
{
    static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = BuildConfiguration();

        // Configure logging
        var loggingConfig = new LoggingConfiguration();
        configuration.GetSection("Logging").Bind(loggingConfig);
        Log.Logger = LoggingService.ConfigureLogger(loggingConfig);

        try
        {
            Log.Information("AIKeyMouse Code Generator starting...");

            // Build and run Cocona app
            var builder = CoconaApp.CreateBuilder(args);
            
            // Register configuration
            builder.Services.AddSingleton(configuration);
            
            // Use Serilog
            builder.Host.UseSerilog();
            
            // Register infrastructure services
            builder.Services.AddSingleton<ConfigurationService>();
            builder.Services.AddSingleton<FileService>();
            
            // Register HTTP client for LLM providers
            builder.Services.AddHttpClient();

            var app = builder.Build();

            // Add commands
            app.AddCommand("version", () =>
            {
                Console.WriteLine("AIKeyMouse Code Generator v0.1.0");
                Console.WriteLine("Copyright (c) 2026 AIKeyMouse");
            })
            .WithDescription("Show version information");

            // Run the app
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    /// <summary>
    /// Build configuration from multiple sources
    /// </summary>
    static IConfiguration BuildConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("Config/appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"Config/appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("aikeymouse.config.json", optional: true, reloadOnChange: true) // Project config
            .AddJsonFile(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".aikeymouse",
                    "config.json"),
                optional: true,
                reloadOnChange: true) // User config
            .AddEnvironmentVariables("AIKEYMOUSE_") // Environment variables with prefix
            .Build();
    }
}
