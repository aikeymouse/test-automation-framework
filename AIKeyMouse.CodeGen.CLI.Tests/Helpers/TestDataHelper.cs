namespace AIKeyMouse.CodeGen.CLI.Tests.Helpers;

/// <summary>
/// Helper class to manage test data and output directories
/// </summary>
public class TestDataHelper
{
    private readonly string _testOutputBasePath;

    public TestDataHelper(string testOutputBasePath)
    {
        _testOutputBasePath = testOutputBasePath;
    }

    /// <summary>
    /// Get a unique output directory for a test
    /// </summary>
    public string GetTestOutputDirectory(string testName)
    {
        var testDir = Path.Combine(_testOutputBasePath, testName, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        Directory.CreateDirectory(testDir);
        return testDir;
    }

    /// <summary>
    /// Clean up old test output directories
    /// </summary>
    public void CleanupOldOutputs(int keepLastNRuns = 3)
    {
        if (!Directory.Exists(_testOutputBasePath))
            return;

        var testDirs = Directory.GetDirectories(_testOutputBasePath);
        
        foreach (var testDir in testDirs)
        {
            var runs = Directory.GetDirectories(testDir)
                .OrderByDescending(d => Directory.GetCreationTime(d))
                .Skip(keepLastNRuns);

            foreach (var oldRun in runs)
            {
                try
                {
                    Directory.Delete(oldRun, recursive: true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }

    /// <summary>
    /// Get path to a test data file
    /// </summary>
    public string GetTestDataPath(string relativePath)
    {
        var baseDir = AppContext.BaseDirectory;
        return Path.Combine(baseDir, "TestData", relativePath);
    }

    /// <summary>
    /// Verify a file was generated
    /// </summary>
    public bool FileExists(string directory, string fileName)
    {
        var filePath = Path.Combine(directory, fileName);
        return File.Exists(filePath);
    }

    /// <summary>
    /// Read generated file content
    /// </summary>
    public string ReadGeneratedFile(string directory, string fileName)
    {
        var filePath = Path.Combine(directory, fileName);
        return File.ReadAllText(filePath);
    }
}
