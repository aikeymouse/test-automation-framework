namespace AIKeyMouse.CodeGen.CLI.Models.Parsing;

/// <summary>
/// Parsed feature file information
/// </summary>
public class ParsedFeature
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<ParsedScenario> Scenarios { get; set; } = new();
    public string? FilePath { get; set; }
}

/// <summary>
/// Parsed scenario information
/// </summary>
public class ParsedScenario
{
    public required string Name { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<ParsedStep> Steps { get; set; } = new();
    public ScenarioType Type { get; set; } = ScenarioType.Scenario;
}

/// <summary>
/// Parsed step information
/// </summary>
public class ParsedStep
{
    public required string Keyword { get; set; } // Given, When, Then, And, But
    public required string Text { get; set; }
    public StepType Type { get; set; }
    public string? Argument { get; set; } // Data table or doc string
}

/// <summary>
/// Scenario type
/// </summary>
public enum ScenarioType
{
    Scenario,
    ScenarioOutline,
    Background
}

/// <summary>
/// Step type (normalized from keyword)
/// </summary>
public enum StepType
{
    Given,
    When,
    Then,
    And,
    But
}
