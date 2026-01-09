using Cocona;
using AIKeyMouse.CodeGen.CLI.Models.Configuration;
using AIKeyMouse.CodeGen.CLI.Models.LLM;
using AIKeyMouse.CodeGen.CLI.Services.Infrastructure;
using AIKeyMouse.CodeGen.CLI.Services.LLM;
using AIKeyMouse.CodeGen.CLI.Services.Skills;
using AIKeyMouse.CodeGen.CLI.Services.Parsers;
using Polly;
using Polly.Extensions.Http;

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
            
            // Register LLM configuration
            var cliConfig = new CliConfig();
            configuration.Bind(cliConfig);
            builder.Services.AddSingleton(cliConfig.Llm.Groq);
            builder.Services.AddSingleton(cliConfig.Llm.HuggingFace);
            
            // Register HTTP clients with Polly retry policies
            builder.Services.AddHttpClient<GroqProvider>()
                .AddPolicyHandler(GetRetryPolicy());
                
            builder.Services.AddHttpClient<HuggingFaceProvider>()
                .AddPolicyHandler(GetRetryPolicy());
            
            // Register LLM providers
            builder.Services.AddSingleton<ILlmProvider, GroqProvider>();
            builder.Services.AddSingleton<ILlmProvider, HuggingFaceProvider>();
            builder.Services.AddSingleton<LlmProviderFactory>();
            
            // Register Skills services
            builder.Services.AddSingleton<SkillLoader>();
            builder.Services.AddSingleton<SkillValidator>();
            builder.Services.AddSingleton<PromptBuilder>();
            
            // Register Parsers
            builder.Services.AddSingleton<GherkinParser>();
            builder.Services.AddSingleton<HtmlParser>();

            var app = builder.Build();

            // Add commands
            app.AddCommand("version", () =>
            {
                Console.WriteLine("AIKeyMouse Code Generator v0.1.0");
                Console.WriteLine("Copyright (c) 2026 AIKeyMouse");
            })
            .WithDescription("Show version information");

            // Add test command for LLM integration
            app.AddCommand("test-llm", async ([Option('p', Description = "Test prompt")] string prompt) =>
            {
                var factory = app.Services.GetRequiredService<LlmProviderFactory>();
                
                Console.WriteLine("Testing LLM integration...");
                Console.WriteLine($"Prompt: {prompt}");
                Console.WriteLine();

                try
                {
                    var request = new LlmRequest
                    {
                        Prompt = prompt,
                        SystemMessage = "You are a helpful coding assistant. Provide concise answers.",
                        Temperature = 0.7f,
                        MaxTokens = 500
                    };

                    var response = await factory.GenerateWithFailoverAsync(request);
                    
                    Console.WriteLine($"✅ Provider: {response.Provider}");
                    Console.WriteLine($"✅ Model: {response.Model}");
                    Console.WriteLine($"✅ Tokens: {response.TotalTokens} (prompt: {response.PromptTokens}, completion: {response.CompletionTokens})");
                    Console.WriteLine();
                    Console.WriteLine("Response:");
                    Console.WriteLine(response.Content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error: {ex.Message}");
                    return 1;
                }

                return 0;
            })
            .WithDescription("Test LLM provider integration");

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

    /// <summary>
    /// Get Polly retry policy for HTTP calls
    /// </summary>
    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Log.Warning(
                        "HTTP request failed (attempt {RetryCount}). Waiting {Delay}ms before retry. Reason: {Reason}",
                        retryCount,
                        timespan.TotalMilliseconds,
                        outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase ?? "Unknown");
                });
    }
}
