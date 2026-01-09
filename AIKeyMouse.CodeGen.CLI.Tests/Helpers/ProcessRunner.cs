using System.Diagnostics;
using System.Text;

namespace AIKeyMouse.CodeGen.CLI.Tests.Helpers;

/// <summary>
/// Helper class to execute CLI commands and capture output
/// </summary>
public class ProcessRunner
{
    private readonly string _cliProjectPath;

    public ProcessRunner(string cliProjectPath)
    {
        _cliProjectPath = cliProjectPath;
    }

    /// <summary>
    /// Execute a CLI command and return the result
    /// </summary>
    public async Task<CommandResult> RunAsync(string arguments, int timeoutSeconds = 300)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{_cliProjectPath}\" -- {arguments}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(_cliProjectPath)!
        };

        var output = new StringBuilder();
        var error = new StringBuilder();

        using var process = new Process { StartInfo = startInfo };
        
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                output.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                error.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var completed = await Task.Run(() => process.WaitForExit(timeoutSeconds * 1000));

        if (!completed)
        {
            process.Kill();
            throw new TimeoutException($"Command timed out after {timeoutSeconds} seconds: {arguments}");
        }

        return new CommandResult
        {
            ExitCode = process.ExitCode,
            Output = output.ToString(),
            Error = error.ToString(),
            Success = process.ExitCode == 0
        };
    }
}

/// <summary>
/// Result of a CLI command execution
/// </summary>
public class CommandResult
{
    public int ExitCode { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public bool Success { get; set; }
}
