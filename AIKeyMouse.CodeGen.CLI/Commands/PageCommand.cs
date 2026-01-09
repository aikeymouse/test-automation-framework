using Cocona;
using AIKeyMouse.CodeGen.CLI.Commands;
using AIKeyMouse.CodeGen.CLI.Models.LLM;
using AIKeyMouse.CodeGen.CLI.Services.Infrastructure;
using AIKeyMouse.CodeGen.CLI.Services.LLM;
using AIKeyMouse.CodeGen.CLI.Services.Skills;
using AIKeyMouse.CodeGen.CLI.Services.Parsers;
using AIKeyMouse.CodeGen.CLI.Services.CodeGeneration;
using Microsoft.Extensions.Logging;

namespace AIKeyMouse.CodeGen.CLI.Commands;

/// <summary>
/// Command for generating Page Object Model classes
/// </summary>
public class PageCommand : BaseCommand
{
    private readonly LlmProviderFactory _llmFactory;
    private readonly SkillLoader _skillLoader;
    private readonly PromptBuilder _promptBuilder;
    private readonly HtmlParser _htmlParser;
    private readonly CodeGenerator _codeGenerator;

    public PageCommand(
        ILogger<PageCommand> logger,
        ConfigurationService configService,
        FileService fileService,
        LlmProviderFactory llmFactory,
        SkillLoader skillLoader,
        PromptBuilder promptBuilder,
        HtmlParser htmlParser,
        CodeGenerator codeGenerator)
        : base(logger, configService, fileService)
    {
        _llmFactory = llmFactory;
        _skillLoader = skillLoader;
        _promptBuilder = promptBuilder;
        _htmlParser = htmlParser;
        _codeGenerator = codeGenerator;
    }

    /// <summary>
    /// Generate a page object class
    /// </summary>
    public async Task<int> ExecuteAsync(
        [Option('n', Description = "Page name (e.g., Login)")] string name,
        [Option('u', Description = "URL to parse HTML from")] string? url = null,
        [Option('h', Description = "HTML file path to parse")] string? htmlFile = null,
        [Option('o', Description = "Output directory")] string? output = null,
        [Option("namespace", Description = "Custom namespace")] string? customNamespace = null,
        [Option('s', Description = "Skill file path")] string? skillPath = null,
        [Option('p', Description = "Platform (web, mobile, desktop)")] string platform = "web",
        [Option('c', Description = "Page container CSS selector (e.g., '#myForm', '.main-content')")] string? containerSelector = null)
    {
        DisplayInfo($"Generating Page Object: {name}Page");

        try
        {
            // Load skill
            var skill = await LoadSkillAsync(skillPath, platform);
            
            // Build context for prompt
            var context = new Dictionary<string, object>
            {
                ["pageName"] = name,
                ["platform"] = platform,
                ["namespace"] = customNamespace ?? Config.CodeGeneration.DefaultNamespace ?? "Pages"
            };

            // Parse HTML if URL or file provided
            if (!string.IsNullOrWhiteSpace(url) || !string.IsNullOrWhiteSpace(htmlFile))
            {
                var parsedPage = await ParsePageAsync(url, htmlFile, containerSelector);
                
                // Log parsed elements in detail (debug level)
                Logger.LogDebug("==== PARSED ELEMENTS ====");
                foreach (var element in parsedPage.Elements)
                {
                    Logger.LogDebug("  - {Name} ({Type}): {Locator}", 
                        element.Name, element.Type, element.CssSelector ?? element.XPath);
                }
                Logger.LogDebug("========================");
                
                var elementsList = parsedPage.Elements.Select(e => new Dictionary<string, object>
                {
                    ["name"] = e.Name,
                    ["type"] = e.Type,
                    ["locator"] = e.CssSelector ?? e.XPath
                }).ToList();
                
                context["elements"] = elementsList;

                // Add container selector to context
                if (!string.IsNullOrWhiteSpace(parsedPage.ContainerSelector))
                {
                    context["containerSelector"] = parsedPage.ContainerSelector;
                    context["containerXPath"] = parsedPage.ContainerXPath ?? "";
                    context["containerTag"] = parsedPage.ContainerTag ?? "";
                    Logger.LogInformation("Using page container: {Container} ({Tag} at {XPath})", 
                        parsedPage.ContainerSelector, parsedPage.ContainerTag, parsedPage.ContainerXPath);
                }

                // Debug: Log what we're passing to the template
                Logger.LogDebug("Context elements count: {Count}", elementsList.Count);
                foreach (var elem in elementsList)
                {
                    Logger.LogDebug("Element in context: {Element}", System.Text.Json.JsonSerializer.Serialize(elem));
                }

                if (!string.IsNullOrWhiteSpace(parsedPage.Title))
                {
                    context["pageTitle"] = parsedPage.Title;
                }
            }

            // Build prompt
            DisplayInfo("Building prompt from skill template...");
            var userPrompt = await _promptBuilder.BuildPromptAsync(skill, context);
            var systemPrompt = _promptBuilder.BuildSystemMessage(skill);

            // Generate code using LLM
            DisplayInfo($"Generating code using {skill.Name}...");
            var llmRequest = new LlmRequest
            {
                Prompt = userPrompt,
                SystemMessage = systemPrompt,
                Temperature = (float)(skill.LlmParams?.Temperature ?? Config.Llm.Temperature),
                MaxTokens = skill.LlmParams?.MaxTokens ?? Config.Llm.MaxTokens
            };

            var response = await _llmFactory.GenerateWithFailoverAsync(llmRequest);
            
            DisplayInfo($"✓ Generated by {response.Provider} ({response.Model}) - {response.TotalTokens} tokens");

            // Extract code from response
            var code = _promptBuilder.ExtractCode(response.Content, skill.Output?.CodeExtraction);

            // Add required using statements
            if (skill.Validation?.RequiredUsings != null && skill.Validation.RequiredUsings.Count > 0)
            {
                code = _codeGenerator.AddUsings(code, skill.Validation.RequiredUsings);
            }

            // Format code if enabled
            if (skill.Validation?.AutoFormat ?? Config.CodeGeneration.AutoFormat)
            {
                DisplayInfo("Formatting code...");
                code = _codeGenerator.FormatCode(code);
            }

            // Validate syntax if enabled
            if (skill.Validation?.ValidateSyntax ?? Config.CodeGeneration.ValidateSyntax)
            {
                DisplayInfo("Validating syntax...");
                var (isValid, errors) = _codeGenerator.ValidateSyntax(code);
                
                if (!isValid)
                {
                    DisplayWarning($"Code has {errors.Count} syntax error(s):");
                    foreach (var error in errors)
                    {
                        DisplayError($"  {error}");
                    }
                    
                    if (!await ConfirmActionAsync("Continue with syntax errors?"))
                    {
                        return 1;
                    }
                }
                else
                {
                    DisplaySuccess("✓ Syntax validation passed");
                }
            }

            // Determine output path
            var outputDir = output ?? skill.Output?.DefaultPath ?? "Pages";
            var fileNamePattern = skill.Output?.FileNamePattern ?? "{name}Page.cs";
            var fileName = fileNamePattern.Replace("{name}", name);
            var outputPath = Path.Combine(outputDir, fileName);

            // Write file
            await FileService.WriteFileAsync(outputPath, code);
            
            DisplaySuccess($"✓ Page object generated: {outputPath}");
            DisplayInfo($"Class: {_codeGenerator.GetClassName(code)}");
            DisplayInfo($"Namespace: {_codeGenerator.GetNamespace(code)}");

            return 0;
        }
        catch (Exception ex)
        {
            DisplayError($"Failed to generate page object: {ex.Message}");
            Logger.LogError(ex, "Page generation failed");
            return 1;
        }
    }

    private async Task<Models.Skills.Skill> LoadSkillAsync(string? skillPath, string platform)
    {
        if (!string.IsNullOrWhiteSpace(skillPath))
        {
            DisplayInfo($"Loading custom skill from: {skillPath}");
            return await _skillLoader.LoadSkillAsync(skillPath);
        }

        // Load built-in skill based on platform
        var builtInSkillPath = platform.ToLowerInvariant() switch
        {
            "web" => Path.Combine(AppContext.BaseDirectory, "Skills", "PageObjects", "page-object-web.skill.md"),
            "mobile" => Path.Combine(AppContext.BaseDirectory, "Skills", "PageObjects", "page-object-mobile.skill.json"),
            "desktop" => Path.Combine(AppContext.BaseDirectory, "Skills", "PageObjects", "page-object-desktop.skill.json"),
            _ => throw new ArgumentException($"Unsupported platform: {platform}")
        };

        DisplayInfo($"Using built-in {platform} page object skill");
        return await _skillLoader.LoadSkillAsync(builtInSkillPath);
    }

    private async Task<Models.Parsing.ParsedPage> ParsePageAsync(string? url, string? htmlFile, string? containerSelector)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            DisplayInfo($"Parsing HTML from URL: {url}");
            return await _htmlParser.ParseFromUrlAsync(url, containerSelector);
        }

        if (!string.IsNullOrWhiteSpace(htmlFile))
        {
            DisplayInfo($"Parsing HTML from file: {htmlFile}");
            return _htmlParser.ParseFromFile(htmlFile, containerSelector);
        }

        throw new InvalidOperationException("No URL or HTML file provided");
    }
}
