using System.Text.RegularExpressions;
using AIKeyMouse.CodeGen.CLI.Models.Parsing;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace AIKeyMouse.CodeGen.CLI.Services.Parsers;

/// <summary>
/// Parser for HTML pages to extract elements for page object generation
/// </summary>
public class HtmlParser
{
    private readonly ILogger<HtmlParser> _logger;
    private readonly HttpClient _httpClient;

    public HtmlParser(ILogger<HtmlParser> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Parse HTML from a URL
    /// </summary>
    public async Task<ParsedPage> ParseFromUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching HTML from URL: {Url}", url);

        var html = await _httpClient.GetStringAsync(url, cancellationToken);
        return ParseHtml(html, url);
    }

    /// <summary>
    /// Parse HTML from file
    /// </summary>
    public ParsedPage ParseFromFile(string filePath)
    {
        _logger.LogInformation("Parsing HTML file: {FilePath}", filePath);

        var html = File.ReadAllText(filePath);
        return ParseHtml(html, filePath);
    }

    /// <summary>
    /// Parse HTML content
    /// </summary>
    public ParsedPage ParseHtml(string html, string source)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var page = new ParsedPage
        {
            Url = source,
            Title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText.Trim()
        };

        // Parse all interactive elements
        page.Elements = ParseElements(doc);
        
        // Parse forms
        page.Forms = ParseForms(doc);
        
        // Parse links
        page.Links = ParseLinks(doc);

        _logger.LogInformation(
            "Parsed page with {ElementCount} elements, {FormCount} forms, {LinkCount} links",
            page.Elements.Count, page.Forms.Count, page.Links.Count);

        return page;
    }

    /// <summary>
    /// Parse interactive elements
    /// </summary>
    private List<ParsedElement> ParseElements(HtmlDocument doc)
    {
        var elements = new List<ParsedElement>();

        // Input fields
        var inputs = doc.DocumentNode.SelectNodes("//input");
        if (inputs != null)
        {
            foreach (var input in inputs)
            {
                var type = input.GetAttributeValue("type", "text");
                if (type == "hidden") continue; // Skip hidden fields

                // Use specific type for better code generation
                var elementType = type switch
                {
                    "text" => "text-input",
                    "password" => "password-input",
                    "checkbox" => "checkbox",
                    "radio" => "radio",
                    "submit" => "submit-button",
                    "button" => "button",
                    _ => "input"
                };

                elements.Add(CreateElement(input, elementType));
            }
        }

        // Textareas
        var textareas = doc.DocumentNode.SelectNodes("//textarea");
        if (textareas != null)
        {
            foreach (var textarea in textareas)
            {
                elements.Add(CreateElement(textarea, "textarea"));
            }
        }

        // Buttons
        var buttons = doc.DocumentNode.SelectNodes("//button");
        if (buttons != null)
        {
            foreach (var button in buttons)
            {
                elements.Add(CreateElement(button, "button"));
            }
        }

        // Submit inputs
        var submits = doc.DocumentNode.SelectNodes("//input[@type='submit' or @type='button']");
        if (submits != null)
        {
            foreach (var submit in submits)
            {
                elements.Add(CreateElement(submit, "button"));
            }
        }

        // Select dropdowns
        var selects = doc.DocumentNode.SelectNodes("//select");
        if (selects != null)
        {
            foreach (var select in selects)
            {
                elements.Add(CreateElement(select, "select"));
            }
        }

        // Links
        var links = doc.DocumentNode.SelectNodes("//a[@href]");
        if (links != null)
        {
            foreach (var link in links)
            {
                var href = link.GetAttributeValue("href", "");
                if (string.IsNullOrWhiteSpace(href) || href.StartsWith("#")) continue;

                elements.Add(CreateElement(link, "link"));
            }
        }

        return elements;
    }

    /// <summary>
    /// Parse forms
    /// </summary>
    private List<ParsedForm> ParseForms(HtmlDocument doc)
    {
        var forms = new List<ParsedForm>();
        var formNodes = doc.DocumentNode.SelectNodes("//form");

        if (formNodes == null) return forms;

        foreach (var formNode in formNodes)
        {
            var form = new ParsedForm
            {
                Name = formNode.GetAttributeValue("name", null),
                Id = formNode.GetAttributeValue("id", null),
                Action = formNode.GetAttributeValue("action", null),
                Method = formNode.GetAttributeValue("method", "get")
            };

            // Get all input elements within the form
            var formInputs = formNode.SelectNodes(".//input | .//textarea | .//select | .//button");
            if (formInputs != null)
            {
                foreach (var input in formInputs)
                {
                    var type = input.GetAttributeValue("type", "text");
                    if (type == "hidden") continue;

                    form.Fields.Add(CreateElement(input, input.Name));
                }
            }

            forms.Add(form);
        }

        return forms;
    }

    /// <summary>
    /// Parse links
    /// </summary>
    private List<ParsedLink> ParseLinks(HtmlDocument doc)
    {
        var links = new List<ParsedLink>();
        var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");

        if (linkNodes == null) return links;

        foreach (var linkNode in linkNodes)
        {
            var href = linkNode.GetAttributeValue("href", "");
            if (string.IsNullOrWhiteSpace(href) || href.StartsWith("#")) continue;

            links.Add(new ParsedLink
            {
                Text = linkNode.InnerText.Trim(),
                Href = href,
                CssSelector = GenerateCssSelector(linkNode)
            });
        }

        return links;
    }

    /// <summary>
    /// Create parsed element from HTML node
    /// </summary>
    private ParsedElement CreateElement(HtmlNode node, string elementType)
    {
        var id = node.GetAttributeValue("id", null);
        var name = node.GetAttributeValue("name", null);
        var classAttr = node.GetAttributeValue("class", "");
        var classes = classAttr.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        // Generate a friendly name
        var friendlyName = GenerateFriendlyName(id, name, node.GetAttributeValue("placeholder", null), elementType);

        var element = new ParsedElement
        {
            Name = friendlyName,
            Type = elementType,
            Id = id,
            Classes = classes,
            CssSelector = GenerateCssSelector(node),
            XPath = node.XPath,
            Text = node.InnerText.Trim(),
            Placeholder = node.GetAttributeValue("placeholder", null)
        };

        // Add all attributes
        foreach (var attr in node.Attributes)
        {
            element.Attributes[attr.Name] = attr.Value;
        }

        return element;
    }

    /// <summary>
    /// Generate CSS selector for element
    /// </summary>
    private string GenerateCssSelector(HtmlNode node)
    {
        var id = node.GetAttributeValue("id", null);
        if (!string.IsNullOrWhiteSpace(id))
        {
            return $"#{id}";
        }

        var name = node.GetAttributeValue("name", null);
        if (!string.IsNullOrWhiteSpace(name))
        {
            return $"{node.Name}[name='{name}']";
        }

        var classAttr = node.GetAttributeValue("class", "");
        if (!string.IsNullOrWhiteSpace(classAttr))
        {
            var classes = classAttr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (classes.Length > 0)
            {
                return $"{node.Name}.{string.Join(".", classes)}";
            }
        }

        return node.XPath;
    }

    /// <summary>
    /// Generate friendly name for element
    /// </summary>
    private string GenerateFriendlyName(string? id, string? name, string? placeholder, string elementType)
    {
        // Try ID first
        if (!string.IsNullOrWhiteSpace(id))
        {
            return ToPascalCase(id);
        }

        // Try name attribute
        if (!string.IsNullOrWhiteSpace(name))
        {
            return ToPascalCase(name);
        }

        // Try placeholder
        if (!string.IsNullOrWhiteSpace(placeholder))
        {
            return ToPascalCase(placeholder);
        }

        // Default to element type
        return $"{ToPascalCase(elementType)}Element";
    }

    /// <summary>
    /// Convert string to PascalCase
    /// </summary>
    private string ToPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        // Remove special characters and split by common separators
        var words = Regex.Replace(input, @"[^a-zA-Z0-9\s-_]", "")
            .Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);

        return string.Join("", words.Select(w => 
            char.ToUpper(w[0]) + w.Substring(1).ToLower()));
    }
}
