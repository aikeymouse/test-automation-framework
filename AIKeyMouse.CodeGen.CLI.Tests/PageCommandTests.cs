using Microsoft.VisualStudio.TestTools.UnitTesting;
using AIKeyMouse.CodeGen.CLI.Tests.Helpers;
using FluentAssertions;

namespace AIKeyMouse.CodeGen.CLI.Tests;

[TestClass]
public class PageCommandTests
{
    private static ProcessRunner? _runner;
    private static TestDataHelper? _testData;
    private static string? _cliProjectPath;

    [ClassInitialize]
    public static void ClassSetup(TestContext context)
    {
        // Get path to CLI project
        var baseDir = AppContext.BaseDirectory;
        var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        _cliProjectPath = Path.Combine(projectRoot, "AIKeyMouse.CodeGen.CLI", "AIKeyMouse.CodeGen.CLI.csproj");

        _runner = new ProcessRunner(_cliProjectPath);
        
        var testOutputPath = Path.Combine(projectRoot, "AIKeyMouse.CodeGen.CLI.Tests", "TestOutput");
        _testData = new TestDataHelper(testOutputPath);
        
        // Cleanup old test runs
        _testData.CleanupOldOutputs(keepLastNRuns: 3);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task PageCommand_WithoutHtml_GeneratesBasicPageObject()
    {
        // Arrange
        var outputDir = _testData!.GetTestOutputDirectory(nameof(PageCommand_WithoutHtml_GeneratesBasicPageObject));
        var pageName = "Dashboard";

        // Act
        var result = await _runner!.RunAsync(
            $"page --name {pageName} --output \"{outputDir}\"",
            timeoutSeconds: 300);

        // Assert
        result.Success.Should().BeTrue($"Command should succeed. Output: {result.Output}\nError: {result.Error}");
        result.Output.Should().Contain("Page object generated");
        
        var generatedFile = Path.Combine(outputDir, $"{pageName}Page.cs");
        File.Exists(generatedFile).Should().BeTrue("Generated file should exist");

        var code = File.ReadAllText(generatedFile);
        var (isValid, errors) = CodeValidator.ValidateSyntax(code);
        isValid.Should().BeTrue($"Code should have valid syntax. Errors: {string.Join(", ", errors)}");

        CodeValidator.GetClassName(code).Should().Be($"{pageName}Page");
        CodeValidator.ContainsUsings(code, "OpenQA.Selenium", "SeleniumExtras.PageObjects").Should().BeTrue();
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task PageCommand_WithHtmlFile_ParsesElementsAndGeneratesPageObject()
    {
        // Arrange
        var outputDir = _testData!.GetTestOutputDirectory(nameof(PageCommand_WithHtmlFile_ParsesElementsAndGeneratesPageObject));
        var htmlFile = _testData.GetTestDataPath("Html/login.html");
        var pageName = "Login";

        // Act
        var result = await _runner!.RunAsync(
            $"page --name {pageName} --html-file \"{htmlFile}\" --output \"{outputDir}\"",
            timeoutSeconds: 600);

        // Assert
        result.Success.Should().BeTrue($"Command should succeed. Output: {result.Output}\nError: {result.Error}");
        result.Output.Should().Contain("Parsing HTML from file");
        result.Output.Should().Contain("Page object generated");

        var generatedFile = Path.Combine(outputDir, $"{pageName}Page.cs");
        File.Exists(generatedFile).Should().BeTrue("Generated file should exist");

        var code = File.ReadAllText(generatedFile);
        var (isValid, errors) = CodeValidator.ValidateSyntax(code);
        isValid.Should().BeTrue($"Code should have valid syntax. Errors: {string.Join(", ", errors)}");

        // Verify it contains elements from HTML
        code.Should().Contain("username", "Should contain username element");
        code.Should().Contain("password", "Should contain password element");
        code.Should().Contain("FindsBy", "Should use FindsBy attributes");
        
        var methods = CodeValidator.GetMethodNames(code);
        methods.Should().NotBeEmpty("Should have methods");
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task PageCommand_WithCustomNamespace_UsesSpecifiedNamespace()
    {
        // Arrange
        var outputDir = _testData!.GetTestOutputDirectory(nameof(PageCommand_WithCustomNamespace_UsesSpecifiedNamespace));
        var pageName = "Custom";
        var customNamespace = "MyProject.Pages";

        // Act
        var result = await _runner!.RunAsync(
            $"page --name {pageName} --output \"{outputDir}\" --namespace {customNamespace}",
            timeoutSeconds: 300);

        // Assert
        result.Success.Should().BeTrue($"Command should succeed. Error: {result.Error}");

        var generatedFile = Path.Combine(outputDir, $"{pageName}Page.cs");
        var code = File.ReadAllText(generatedFile);
        
        CodeValidator.GetNamespace(code).Should().Be(customNamespace);
    }

    [TestMethod]
    public async Task PageCommand_WithoutName_ReturnsError()
    {
        // Act
        var result = await _runner!.RunAsync("page", timeoutSeconds: 30);

        // Assert
        result.Success.Should().BeFalse("Command should fail without required name parameter");
        (result.Error + result.Output).Should().Contain("name");
    }
}
