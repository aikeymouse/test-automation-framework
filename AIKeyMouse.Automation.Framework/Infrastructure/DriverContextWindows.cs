using OpenQA.Selenium.Edge;

namespace AIKeyMouse.Automation.Framework.Infrastructure;

public partial class DriverContext
{
    private UIAutomationApplication? _application;
    private UIAutomationWindow? _window;

    public UIAutomationApplication? Application => _application;
    public UIAutomationWindow? Window => _window;

    public void StartWindowsApplication(string applicationPath, string? arguments = null)
    {
        _application = UIAutomationApplication.Launch(applicationPath, arguments);
        InitializeWindow();
    }

    private void InitializeWindow()
    {
        if (_application != null)
        {
            _window = _application.GetMainWindow();
        }
    }

    public void AttachToWindowsApplication(int processId)
    {
        _application = UIAutomationApplication.Attach(processId);
        InitializeWindow();
    }

    public void AttachToWindowsApplication(string executableName)
    {
        _application = UIAutomationApplication.Attach(executableName);
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
        _application?.Dispose();
        _application = null;
        _window = null;
    }
}
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
