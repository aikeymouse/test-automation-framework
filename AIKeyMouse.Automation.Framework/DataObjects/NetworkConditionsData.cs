using OpenQA.Selenium.DevTools.V143.Network;

namespace AIKeyMouse.Automation.Framework.DataObjects;

public class NetworkConditionsData
{
    public bool? Offline { get; set; }
    public double? Latency { get; set; }
    public double? DownloadThroughput { get; set; }
    public double? UploadThroughput { get; set; }
    public ConnectionType? ConnectionType { get; set; }
}