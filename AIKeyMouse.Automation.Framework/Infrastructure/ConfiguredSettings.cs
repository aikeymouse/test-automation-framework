using System.Configuration;

namespace AIKeyMouse.Automation.Framework.Infrastructure;

public sealed class ConfiguredSettings
{
    private static readonly Lazy<ConfiguredSettings> lazy =
        new(() => new ConfiguredSettings());
    public static ConfiguredSettings Instance => lazy.Value;

    public bool OpenDevTools { get; set; } = false;
    public string WindowSize { get; set; } = "1920,1080";
    public bool IgnoreUnhandledPrompt { get; set; } = true;
    public bool DriverLog { get; set; } = false;
    public string TestResultsPath { get; set; } = "TestResults";
    public double ShortTimeout { get; set; } = 5.0;
    public double LongTimeout { get; set; } = 30.0;
    public string WebView2DebuggerAddress { get; set; } = "localhost:9222";

    private ConfiguredSettings()
    {
        OpenDevTools = GetConfigurationSetting("OpenDevTools").Equals("true", StringComparison.CurrentCultureIgnoreCase);
        WindowSize = GetConfigurationSetting("WindowSize");
        IgnoreUnhandledPrompt = GetConfigurationSetting("IgnoreUnhandledPrompt").Equals("true", StringComparison.CurrentCultureIgnoreCase);
        DriverLog = GetConfigurationSetting("DriverLog").Equals("true", StringComparison.CurrentCultureIgnoreCase);
        TestResultsPath = GetConfigurationSetting("TestResultsPath");
        ShortTimeout = Convert.ToDouble(GetConfigurationSetting("ShortTimeout"));
        LongTimeout = Convert.ToDouble(GetConfigurationSetting("LongTimeout"));
        WebView2DebuggerAddress = GetConfigurationSetting("WebView2DebuggerAddress");
    }

    private static string GetConfigurationSetting(string key)
    {
        return ConfigurationManager.AppSettings[key] ?? string.Empty;
    }
}