using FlaUI.Core;
using FlaUI.UIA3;
using OpenQA.Selenium.Edge;
using FlaUI.Core.AutomationElements;

namespace AIKeyMouse.Automation.Framework.Infrastructure;

public partial class DriverContext
{
    private Application? _application;
    private UIA3Automation? _automation;
    private Window? _window;

    public Application? Application => _application;
    public UIA3Automation? Automation => _automation;
    public Window? Window => _window;

    public void StartWindowsApplication(string applicationPath, string? arguments = null)
    {
        if (string.IsNullOrWhiteSpace(applicationPath))
        {
            throw new ArgumentException("Application path cannot be null or empty.", nameof(applicationPath));
        }

        if (!File.Exists(applicationPath))
        {
            throw new FileNotFoundException($"Application not found at path: {applicationPath}");
        }

        _automation = new UIA3Automation();
        
        if (string.IsNullOrWhiteSpace(arguments))
        {
            _application = Application.Launch(applicationPath);
        }
        else
        {
            _application = Application.Launch(applicationPath, arguments);
        }
        
        InitializeWindow();
    }

    private void InitializeWindow()
    {
        if (_application != null && _automation != null)
        {
            _window = _application.GetMainWindow(_automation);
        }
    }

    public void AttachToWindowsApplication(int processId)
    {
        if (processId <= 0)
        {
            throw new ArgumentException("Process ID must be greater than zero.", nameof(processId));
        }

        _automation = new UIA3Automation();
        _application = Application.Attach(processId);
        InitializeWindow();
    }

    public void AttachToWindowsApplication(string executableName)
    {
        if (string.IsNullOrWhiteSpace(executableName))
        {
            throw new ArgumentException("Executable name cannot be null or empty.", nameof(executableName));
        }

        _automation = new UIA3Automation();
        _application = Application.Attach(executableName);
        InitializeWindow();
    }

    /// <summary>
    /// Attaches to a WebView2 control in the Windows application using Edge WebDriver.
    /// Requires the Windows application to have remote debugging enabled on the specified port.
    /// To enable in your app: --remote-debugging-port=9222
    /// </summary>
    /// <param name="debuggingPort">The port where WebView2 remote debugging is listening (default: 9222)</param>
    public void AttachToWebView2(int debuggingPort = 9222)
    {
        if (_webDriver != null)
        {
            throw new InvalidOperationException("WebDriver is already attached. Close it before attaching again.");
        }

        var edgeOptions = new EdgeOptions();
        edgeOptions.DebuggerAddress = $"localhost:{debuggingPort}";
        
        var edgeDriverService = EdgeDriverService.CreateDefaultService();
        _webDriver = new EdgeDriver(edgeDriverService, edgeOptions);
    }

    public void CloseWindowsApplication()
    {
        // Close WebDriver (WebView2 or browser) if attached
        if (_webDriver != null)
        {
            _webDriver.Quit();
            _webDriver.Dispose();
            _webDriver = null;
        }

        // Close Windows application
        if (_application != null)
        {
            _application.Close();
            _application.Dispose();
            _application = null;
        }

        _automation?.Dispose();
        _automation = null;
        _window = null;
    }
}
