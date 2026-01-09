using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AIKeyMouse.CodeGen.CLI.Tests.Helpers;

/// <summary>
/// Helper class to validate generated C# code
/// </summary>
public class CodeValidator
{
    /// <summary>
    /// Validate that code has no syntax errors
    /// </summary>
    public static (bool IsValid, List<string> Errors) ValidateSyntax(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var diagnostics = tree.GetDiagnostics();
        var errors = diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => $"Line {d.Location.GetLineSpan().StartLinePosition.Line + 1}: {d.GetMessage()}")
            .ToList();

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Check if code contains expected using statements
    /// </summary>
    public static bool ContainsUsings(string code, params string[] expectedUsings)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var actualUsings = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax>()
            .Select(u => u.Name?.ToString() ?? string.Empty)
            .ToList();

        return expectedUsings.All(expected => 
            actualUsings.Any(actual => actual.Contains(expected)));
    }

    /// <summary>
    /// Get namespace from code
    /// </summary>
    public static string? GetNamespace(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        
        var namespaceDecl = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax>()
            .FirstOrDefault();
            
        if (namespaceDecl != null)
            return namespaceDecl.Name.ToString();

        var fileScopedNamespace = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax>()
            .FirstOrDefault();
            
        return fileScopedNamespace?.Name.ToString();
    }

    /// <summary>
    /// Get class name from code
    /// </summary>
    public static string? GetClassName(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        
        var classDecl = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .FirstOrDefault();
            
        return classDecl?.Identifier.Text;
    }

    /// <summary>
    /// Get all method names from code
    /// </summary>
    public static List<string> GetMethodNames(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        
        return root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .Select(m => m.Identifier.Text)
            .ToList();
    }

    /// <summary>
    /// Check if code contains specific attributes
    /// </summary>
    public static bool ContainsAttribute(string code, string attributeName)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        
        return root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax>()
            .Any(a => a.Name.ToString().Contains(attributeName));
    }
}
