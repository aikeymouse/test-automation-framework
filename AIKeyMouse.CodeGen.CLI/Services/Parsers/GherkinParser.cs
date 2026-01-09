using AIKeyMouse.CodeGen.CLI.Models.Parsing;
using AIKeyMouse.CodeGen.CLI.Services.Infrastructure;
using Gherkin;
using Gherkin.Ast;
using Microsoft.Extensions.Logging;

namespace AIKeyMouse.CodeGen.CLI.Services.Parsers;

/// <summary>
/// Parser for Gherkin feature files
/// </summary>
public class GherkinParser
{
    private readonly FileService _fileService;
    private readonly ILogger<GherkinParser> _logger;
    private readonly Parser _gherkinParser;

    public GherkinParser(FileService fileService, ILogger<GherkinParser> logger)
    {
        _fileService = fileService;
        _logger = logger;
        _gherkinParser = new Parser();
    }

    /// <summary>
    /// Parse a feature file
    /// </summary>
    public async Task<ParsedFeature> ParseFeatureFileAsync(string filePath)
    {
        _logger.LogInformation("Parsing feature file: {FilePath}", filePath);

        var content = await _fileService.ReadFileAsync(filePath);
        
        var gherkinDocument = _gherkinParser.Parse(content);

        if (gherkinDocument.Feature == null)
        {
            throw new InvalidOperationException($"No feature found in {filePath}");
        }

        var feature = gherkinDocument.Feature;

        var parsedFeature = new ParsedFeature
        {
            Name = feature.Name,
            Description = feature.Description?.Trim(),
            Tags = feature.Tags.Select(t => t.Name.TrimStart('@')).ToList(),
            FilePath = filePath
        };

        // Parse scenarios
        foreach (var child in feature.Children)
        {
            if (child is Scenario scenario)
            {
                parsedFeature.Scenarios.Add(ParseScenario(scenario));
            }
            else if (child is Background background)
            {
                parsedFeature.Scenarios.Add(new ParsedScenario
                {
                    Name = background.Name,
                    Type = ScenarioType.Background,
                    Steps = background.Steps.Select(ParseStep).ToList()
                });
            }
        }

        _logger.LogInformation("Parsed feature '{FeatureName}' with {ScenarioCount} scenarios",
            parsedFeature.Name, parsedFeature.Scenarios.Count);

        return parsedFeature;
    }

    /// <summary>
    /// Parse a scenario
    /// </summary>
    private ParsedScenario ParseScenario(Scenario scenario)
    {
        return new ParsedScenario
        {
            Name = scenario.Name,
            Type = ScenarioType.Scenario,
            Tags = scenario.Tags.Select(t => t.Name.TrimStart('@')).ToList(),
            Steps = scenario.Steps.Select(ParseStep).ToList()
        };
    }

    /// <summary>
    /// Parse a step
    /// </summary>
    private ParsedStep ParseStep(Step step)
    {
        var stepType = step.Keyword.Trim() switch
        {
            "Given" => StepType.Given,
            "When" => StepType.When,
            "Then" => StepType.Then,
            "And" => StepType.And,
            "But" => StepType.But,
            _ => StepType.And
        };

        string? argument = null;
        
        if (step.Argument is DocString docString)
        {
            argument = docString.Content;
        }
        else if (step.Argument is DataTable dataTable)
        {
            var rows = dataTable.Rows.Select(r => string.Join(" | ", r.Cells.Select(c => c.Value)));
            argument = string.Join("\n", rows);
        }

        return new ParsedStep
        {
            Keyword = step.Keyword.Trim(),
            Text = step.Text,
            Type = stepType,
            Argument = argument
        };
    }

    /// <summary>
    /// Get all steps from a feature as formatted text
    /// </summary>
    public string GetStepsAsText(ParsedFeature feature, string? scenarioName = null)
    {
        var scenarios = string.IsNullOrWhiteSpace(scenarioName)
            ? feature.Scenarios
            : feature.Scenarios.Where(s => s.Name.Equals(scenarioName, StringComparison.OrdinalIgnoreCase));

        var steps = scenarios
            .Where(s => s.Type != ScenarioType.Background)
            .SelectMany(s => s.Steps)
            .Select(s => $"{s.Keyword} {s.Text}");

        return string.Join("\n", steps);
    }

    /// <summary>
    /// Extract page object names from scenario steps
    /// </summary>
    public List<string> ExtractPageObjectNames(ParsedFeature feature)
    {
        var pageNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var scenario in feature.Scenarios)
        {
            foreach (var step in scenario.Steps)
            {
                // Look for common patterns like "I am on the Login page"
                var words = step.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                for (int i = 0; i < words.Length - 1; i++)
                {
                    if (words[i].Equals("page", StringComparison.OrdinalIgnoreCase) && i > 0)
                    {
                        var pageName = words[i - 1];
                        if (char.IsUpper(pageName[0]))
                        {
                            pageNames.Add(pageName);
                        }
                    }
                }
            }
        }

        _logger.LogDebug("Extracted {Count} page object names from feature", pageNames.Count);
        return pageNames.ToList();
    }
}
