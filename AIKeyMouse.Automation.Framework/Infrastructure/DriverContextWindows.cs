using FlaUI.Core;
using FlaUI.UIA3;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using FlaUI.Core.AutomationElements;
using AIKeyMouse.Automation.Framework.Extensions;

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
    public void AttachToWebView2()
    {
        if (_webDriver != null)
        {
            throw new InvalidOperationException("WebDriver is already attached. Close it before attaching again.");
        }

        EdgeOptions edgeOptions = new()
        {
            UseWebView = true,
            DebuggerAddress = ConfiguredSettings.Instance.WebView2DebuggerAddress,
            UnhandledPromptBehavior = ConfiguredSettings.Instance.IgnoreUnhandledPrompt ? UnhandledPromptBehavior.Dismiss : UnhandledPromptBehavior.Default,
            PageLoadStrategy = PageLoadStrategy.Normal
        };

        var filePath = this.GetResultsFilePath("edgedriver.log");
        
        var edgeDriverService = EdgeDriverService.CreateDefaultService(EdgeDriverPath);
        edgeDriverService.LogPath = filePath;
        edgeDriverService.EnableAppendLog = ConfiguredSettings.Instance.DriverLog;
        _webDriver = new EdgeDriver(edgeDriverService, edgeOptions);
    }

    private string EdgeDriverPath
    {
        get
        {
            string? edgeDriverPath;
            
            // Get WebView2 runtime version
            string? webView2Version = GetWebView2RuntimeVersion();
            
            try
            {
                if (!string.IsNullOrEmpty(webView2Version))
                {
                    // Try to get matching driver for WebView2 version
                    edgeDriverPath = new FileInfo(
                        new WebDriverManager.DriverManager().SetUpDriver(
                            new WebDriverManager.DriverConfigs.Impl.EdgeConfig(),
                            webView2Version)
                            )?.Directory?.FullName;
                }
                else
                {
                    // Fallback to matching browser strategy
                    edgeDriverPath = new FileInfo(
                        new WebDriverManager.DriverManager().SetUpDriver(
                            new WebDriverManager.DriverConfigs.Impl.EdgeConfig(),
                            WebDriverManager.Helpers.VersionResolveStrategy.MatchingBrowser)
                            )?.Directory?.FullName;
                }
            }
            catch (Exception)
            {
                // Final fallback to latest
                edgeDriverPath = new FileInfo(
                    new WebDriverManager.DriverManager().SetUpDriver(
                        new WebDriverManager.DriverConfigs.Impl.EdgeConfig(),
                        WebDriverManager.Helpers.VersionResolveStrategy.Latest)
                        )?.Directory?.FullName;
            }

            if (edgeDriverPath == null || !Directory.Exists(edgeDriverPath))
            {
                throw new DirectoryNotFoundException("Edge Driver path could not be determined.");
            }

            return edgeDriverPath;
        }
    }

    private static string? GetWebView2RuntimeVersion()
    {
        try
        {
            // Check Program Files (x64)
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string webView2Path = Path.Combine(programFiles, "Microsoft", "EdgeWebView", "Application");
            
            if (Directory.Exists(webView2Path))
            {
                // Get version directories (e.g., "120.0.2210.121")
                var versionDirs = Directory.GetDirectories(webView2Path)
                    .Select(d => new DirectoryInfo(d).Name)
                    .Where(name => System.Text.RegularExpressions.Regex.IsMatch(name, @"^\d+\.\d+\.\d+\.\d+$"))
                    .OrderByDescending(v => v)
                    .ToList();
                
                // Check if msedgewebview2.exe exists in the version directory
                foreach (var versionDir in versionDirs)
                {
                    string exePath = Path.Combine(webView2Path, versionDir, "msedgewebview2.exe");
                    if (File.Exists(exePath))
                    {
                        // Get file version from the executable
                        var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath);
                        return fileVersionInfo.FileVersion ?? versionDir;
                    }
                }
            }

            // Check Program Files (x86) if not found
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string webView2PathX86 = Path.Combine(programFilesX86, "Microsoft", "EdgeWebView", "Application");
            
            if (Directory.Exists(webView2PathX86))
            {
                var versionDirs = Directory.GetDirectories(webView2PathX86)
                    .Select(d => new DirectoryInfo(d).Name)
                    .Where(name => System.Text.RegularExpressions.Regex.IsMatch(name, @"^\d+\.\d+\.\d+\.\d+$"))
                    .OrderByDescending(v => v)
                    .ToList();
                
                foreach (var versionDir in versionDirs)
                {
                    string exePath = Path.Combine(webView2PathX86, versionDir, "msedgewebview2.exe");
                    if (File.Exists(exePath))
                    {
                        var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath);
                        return fileVersionInfo.FileVersion ?? versionDir;
                    }
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
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
