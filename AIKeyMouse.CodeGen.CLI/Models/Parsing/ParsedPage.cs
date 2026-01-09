namespace AIKeyMouse.CodeGen.CLI.Models.Parsing;

/// <summary>
/// Parsed HTML page information
/// </summary>
public class ParsedPage
{
    public required string Url { get; set; }
    public string? Title { get; set; }
    public string? ContainerSelector { get; set; }
    public string? ContainerType { get; set; } // CssSelector, XPath, etc.
    public string? ContainerXPath { get; set; } // XPath of the container element
    public string? ContainerTag { get; set; } // HTML tag of the container (form, div, etc.)
    public List<ParsedElement> Elements { get; set; } = new();
    public List<ParsedForm> Forms { get; set; } = new();
    public List<ParsedLink> Links { get; set; } = new();
}

/// <summary>
/// Parsed HTML element
/// </summary>
public class ParsedElement
{
    public required string Name { get; set; }
    public required string Type { get; set; } // input, button, select, textarea, etc.
    public string? Id { get; set; }
    public List<string> Classes { get; set; } = new();
    public string? XPath { get; set; }
    public string? CssSelector { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
    public string? Text { get; set; }
    public string? Placeholder { get; set; }
}

/// <summary>
/// Parsed form information
/// </summary>
public class ParsedForm
{
    public string? Name { get; set; }
    public string? Id { get; set; }
    public string? Action { get; set; }
    public string? Method { get; set; }
    public List<ParsedElement> Fields { get; set; } = new();
}

/// <summary>
/// Parsed link information
/// </summary>
public class ParsedLink
{
    public required string Text { get; set; }
    public required string Href { get; set; }
    public string? CssSelector { get; set; }
}
