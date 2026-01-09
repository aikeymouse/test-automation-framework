using Microsoft.VisualStudio.TestTools.UnitTesting;
using AIKeyMouse.CodeGen.CLI.Tests.Helpers;
using FluentAssertions;

namespace AIKeyMouse.CodeGen.CLI.Tests;

[TestClass]
public class StepsCommandTests
{
    private static ProcessRunner? _runner;
    private static TestDataHelper? _testData;

    [ClassInitialize]
    public static void ClassSetup(TestContext context)
    {
        var baseDir = AppContext.BaseDirectory;
        var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        var cliProjectPath = Path.Combine(projectRoot, "AIKeyMouse.CodeGen.CLI", "AIKeyMouse.CodeGen.CLI.csproj");

        _runner = new ProcessRunner(cliProjectPath);
        
        var testOutputPath = Path.Combine(projectRoot, "AIKeyMouse.CodeGen.CLI.Tests", "TestOutput");
        _testData = new TestDataHelper(testOutputPath);
        
        _testData.CleanupOldOutputs(keepLastNRuns: 3);
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task StepsCommand_WithFeatureFile_GeneratesStepDefinitions()
    {
        // Arrange
        var outputDir = _testData!.GetTestOutputDirectory(nameof(StepsCommand_WithFeatureFile_GeneratesStepDefinitions));
        var featureFile = _testData.GetTestDataPath("Features/login.feature");

        // Act
        var result = await _runner!.RunAsync(
            $"steps --feature \"{featureFile}\" --output \"{outputDir}\"",
            timeoutSeconds: 600);

        // Assert
        result.Success.Should().BeTrue($"Command should succeed. Output: {result.Output}\nError: {result.Error}");
        result.Output.Should().Contain("Step Definitions from");
        result.Output.Should().Contain("Parsing feature file");

        // Find generated step definition file
        var files = Directory.GetFiles(outputDir, "*Steps.cs");
        files.Should().NotBeEmpty("Should generate at least one step definition file");

        var code = File.ReadAllText(files[0]);
        var (isValid, errors) = CodeValidator.ValidateSyntax(code);
        isValid.Should().BeTrue($"Code should have valid syntax. Errors: {string.Join(", ", errors)}");

        // Verify Reqnroll attributes
        code.Should().Contain("Given", "Should contain Given steps");
        code.Should().Contain("When", "Should contain When steps");
        code.Should().Contain("Then", "Should contain Then steps");
        
        CodeValidator.ContainsAttribute(code, "Given").Should().BeTrue();
        CodeValidator.ContainsAttribute(code, "When").Should().BeTrue();
        CodeValidator.ContainsAttribute(code, "Then").Should().BeTrue();
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task StepsCommand_WithSpecificScenario_GeneratesOnlyThatScenario()
    {
        // Arrange
        var outputDir = _testData!.GetTestOutputDirectory(nameof(StepsCommand_WithSpecificScenario_GeneratesOnlyThatScenario));
        var featureFile = _testData.GetTestDataPath("Features/login.feature");
        var scenarioName = "Successful login";

        // Act
        var result = await _runner!.RunAsync(
            $"steps --feature \"{featureFile}\" --output \"{outputDir}\" --scenario \"{scenarioName}\"",
            timeoutSeconds: 600);

        // Assert
        result.Success.Should().BeTrue($"Command should succeed. Output: {result.Output}\nError: {result.Error}");
        result.Output.Should().Contain("Parsing feature file");

        var files = Directory.GetFiles(outputDir, "*Steps.cs");
        files.Should().NotBeEmpty();

        var code = File.ReadAllText(files[0]);
        var (isValid, _) = CodeValidator.ValidateSyntax(code);
        isValid.Should().BeTrue();
    }

    [TestMethod]
    [TestCategory("Integration")]
    public async Task StepsCommand_WithCustomNamespace_UsesSpecifiedNamespace()
    {
        // Arrange
        var outputDir = _testData!.GetTestOutputDirectory(nameof(StepsCommand_WithCustomNamespace_UsesSpecifiedNamespace));
        var featureFile = _testData.GetTestDataPath("Features/login.feature");
        var customNamespace = "MyProject.StepDefinitions";

        // Act
        var result = await _runner!.RunAsync(
            $"steps --feature \"{featureFile}\" --output \"{outputDir}\" --namespace {customNamespace}",
            timeoutSeconds: 600);

        // Assert
        result.Success.Should().BeTrue($"Command should succeed. Output: {result.Output}\nError: {result.Error}");

        var files = Directory.GetFiles(outputDir, "*Steps.cs");
        var code = File.ReadAllText(files[0]);
        
        CodeValidator.GetNamespace(code).Should().Be(customNamespace);
    }

    [TestMethod]
    public async Task StepsCommand_WithoutFeatureFile_ReturnsError()
    {
        // Act
        var result = await _runner!.RunAsync("steps", timeoutSeconds: 30);

        // Assert
        result.Success.Should().BeFalse("Command should fail without required feature parameter");
        (result.Error + result.Output).Should().Contain("feature");
    }
}
