using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Accessibility;

namespace AIKeyMouse.Automation.Framework.Infrastructure;

/// <summary>
/// Wrapper for Windows UI Automation Application/Process
/// </summary>
public class UIAutomationApplication : IDisposable
{
    private readonly System.Diagnostics.Process _process;
    private readonly CUIAutomation8 _automation;
    
    public int ProcessId => _process.Id;
    public string ProcessName => _process.ProcessName;

    private UIAutomationApplication(System.Diagnostics.Process process, CUIAutomation8 automation)
    {
        _process = process;
        _automation = automation;
    }

    public static UIAutomationApplication Launch(string applicationPath, string? arguments = null)
    {
        if (string.IsNullOrWhiteSpace(applicationPath))
        {
            throw new ArgumentException("Application path cannot be null or empty.", nameof(applicationPath));
        }

        if (!File.Exists(applicationPath))
        {
            throw new FileNotFoundException($"Application not found at path: {applicationPath}");
        }

        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = applicationPath,
            Arguments = arguments ?? string.Empty,
            UseShellExecute = true
        };

        var process = System.Diagnostics.Process.Start(startInfo) 
            ?? throw new InvalidOperationException($"Failed to start application: {applicationPath}");

        // Wait for input idle to ensure the main window is ready
        process.WaitForInputIdle(5000);

        var automation = new CUIAutomation8();
        return new UIAutomationApplication(process, automation);
    }

    public static UIAutomationApplication Attach(int processId)
    {
        if (processId <= 0)
        {
            throw new ArgumentException("Process ID must be greater than zero.", nameof(processId));
        }

        var process = System.Diagnostics.Process.GetProcessById(processId);
        var automation = new CUIAutomation8();
        return new UIAutomationApplication(process, automation);
    }

    public static UIAutomationApplication Attach(string executableName)
    {
        if (string.IsNullOrWhiteSpace(executableName))
        {
            throw new ArgumentException("Executable name cannot be null or empty.", nameof(executableName));
        }

        var processes = System.Diagnostics.Process.GetProcessesByName(
            executableName.Replace(".exe", "", StringComparison.OrdinalIgnoreCase));

        if (processes.Length == 0)
        {
            throw new InvalidOperationException($"No process found with name: {executableName}");
        }

        var process = processes[0]; // Get first matching process
        var automation = new CUIAutomation8();
        return new UIAutomationApplication(process, automation);
    }

    public UIAutomationWindow GetMainWindow(int timeoutSeconds = 5)
    {
        var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
        
        while (DateTime.Now < endTime)
        {
            _process.Refresh();
            
            if (_process.MainWindowHandle != IntPtr.Zero)
            {
                var element = _automation.ElementFromHandle(_process.MainWindowHandle);
                if (element != null)
                {
                    return new UIAutomationWindow(element, _automation);
                }
            }

            Thread.Sleep(100);
        }

        throw new InvalidOperationException($"Could not find main window for process {_process.ProcessName}");
    }

    public void Close()
    {
        if (!_process.HasExited)
        {
            _process.CloseMainWindow();
            if (!_process.WaitForExit(5000))
            {
                _process.Kill();
            }
        }
    }

    public void Dispose()
    {
        Close();
        _process.Dispose();
        Marshal.ReleaseComObject(_automation);
        GC.SuppressFinalize(this);
    }
}
