using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using AIKeyMouse.Automation.Framework.Extensions;

namespace AIKeyMouse.Automation.Framework.Infrastructure;

public partial class DriverContext
{
    private IWebDriver? _webDriver;
    public IWebDriver? WebDriver => _webDriver;
    public event EventHandler<DriverOptionsSetEventArgs>? DriverOptionsSet;

    public void StartBrowserDriver()
    {
        var filePath = this.GetResultsFilePath("chromedriver.log");

        var service = ChromeDriverService.CreateDefaultService(ChromeDriverPath);
        service.LogPath = filePath;
        service.EnableAppendLog = ConfiguredSettings.Instance.DriverLog;

        _webDriver = new ChromeDriver(
            service,
            SetDriverOptions(_chromeOptions));
    }

    private ChromeOptions _chromeOptions
    {
        get
        {
            ChromeOptions options = new();
            options.AddUserProfilePreference("profile.default_content_settings.popups", 0);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddArguments("diable-extensions");

            options.AcceptInsecureCertificates = true;
            options.PageLoadStrategy = PageLoadStrategy.Normal;
            options.UnhandledPromptBehavior = ConfiguredSettings.Instance.IgnoreUnhandledPrompt ? UnhandledPromptBehavior.Dismiss : UnhandledPromptBehavior.Default;
            
            return options;
        }
    }

    private T SetDriverOptions<T>(T options) where T : DriverOptions
    {
        DriverOptionsSet?.Invoke(this, new DriverOptionsSetEventArgs(options));
        return options;
    }

    private string ChromeDriverPath
    {
        get
        {
            string? chromeDriverPath;
            try
            {
                chromeDriverPath = new FileInfo(
                    new WebDriverManager.DriverManager().SetUpDriver(
                        new WebDriverManager.DriverConfigs.Impl.ChromeConfig(),
                        WebDriverManager.Helpers.VersionResolveStrategy.MatchingBrowser)
                        )?.Directory?.FullName;
            }
            catch (Exception)
            {
                chromeDriverPath = new FileInfo(
                    new WebDriverManager.DriverManager().SetUpDriver(
                        new WebDriverManager.DriverConfigs.Impl.ChromeConfig(),
                        WebDriverManager.Helpers.VersionResolveStrategy.Latest)
                        )?.Directory?.FullName;
            }

            if (chromeDriverPath == null || !Directory.Exists(chromeDriverPath))
            {
                throw new DirectoryNotFoundException($"ChromeDriver path not found: {chromeDriverPath}");
            }
            return chromeDriverPath;
        }
    }
}