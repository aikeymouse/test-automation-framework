using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.Extensions.Logging;

namespace AIKeyMouse.CodeGen.CLI.Services.CodeGeneration;

/// <summary>
/// Service for code generation, validation, and formatting using Roslyn
/// </summary>
public class CodeGenerator
{
    private readonly ILogger<CodeGenerator> _logger;

    public CodeGenerator(ILogger<CodeGenerator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validate C# syntax
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidateSyntax(string code)
    {
        var errors = new List<string>();

        try
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var diagnostics = tree.GetDiagnostics();

            foreach (var diagnostic in diagnostics)
            {
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    errors.Add($"Line {diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1}: {diagnostic.GetMessage()}");
                }
            }

            var isValid = errors.Count == 0;
            
            if (!isValid)
            {
                _logger.LogWarning("Code validation failed with {ErrorCount} errors", errors.Count);
            }

            return (isValid, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate syntax");
            errors.Add($"Syntax validation error: {ex.Message}");
            return (false, errors);
        }
    }

    /// <summary>
    /// Format C# code using Roslyn
    /// </summary>
    public string FormatCode(string code)
    {
        try
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            // Create workspace for formatting
            var workspace = new AdhocWorkspace();
            var formattedRoot = Formatter.Format(root, workspace);

            return formattedRoot.ToFullString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format code, returning original");
            return code;
        }
    }

    /// <summary>
    /// Add missing using statements
    /// </summary>
    public string AddUsings(string code, List<string> requiredUsings)
    {
        try
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var existingUsings = root.Usings.Select(u => u.Name.ToString()).ToHashSet();

            var newUsings = requiredUsings
                .Where(u => !existingUsings.Contains(u))
                .Select(u => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(u)))
                .ToList();

            if (newUsings.Count == 0)
            {
                return code;
            }

            var updatedRoot = root.AddUsings(newUsings.ToArray());
            return updatedRoot.ToFullString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add using statements");
            return code;
        }
    }

    /// <summary>
    /// Extract namespace from code
    /// </summary>
    public string? GetNamespace(string code)
    {
        try
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var namespaceDecl = root.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault();

            if (namespaceDecl != null)
            {
                return namespaceDecl.Name.ToString();
            }

            var fileScopedNamespace = root.DescendantNodes()
                .OfType<FileScopedNamespaceDeclarationSyntax>()
                .FirstOrDefault();

            return fileScopedNamespace?.Name.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract namespace");
            return null;
        }
    }

    /// <summary>
    /// Extract class name from code
    /// </summary>
    public string? GetClassName(string code)
    {
        try
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var classDecl = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();

            return classDecl?.Identifier.Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract class name");
            return null;
        }
    }

    /// <summary>
    /// Get all syntax errors with line numbers
    /// </summary>
    public List<string> GetSyntaxErrors(string code)
    {
        var errors = new List<string>();

        try
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var diagnostics = tree.GetDiagnostics();

            foreach (var diagnostic in diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
            {
                var lineSpan = diagnostic.Location.GetLineSpan();
                errors.Add($"Line {lineSpan.StartLinePosition.Line + 1}, Column {lineSpan.StartLinePosition.Character + 1}: {diagnostic.GetMessage()}");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Parse error: {ex.Message}");
        }

        return errors;
    }
}
