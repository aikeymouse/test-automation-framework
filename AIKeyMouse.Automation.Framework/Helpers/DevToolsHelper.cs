using OpenQA.Selenium.DevTools;
using AIKeyMouse.Automation.Framework.Infrastructure;
using AIKeyMouse.Automation.Framework.DataObjects;
using OpenQA.Selenium.DevTools.V141.Network;

namespace AIKeyMouse.Automation.Framework.Helpers;

public class DevToolsHelper
{
    private IDevTools _devTools;
    private DevToolsSession _session;

    public DevToolsHelper(DriverContext driverContext)
    {
        _devTools = driverContext.WebDriver as IDevTools ?? throw new InvalidOperationException("WebDriver does not support DevTools.");
        _session = _devTools.GetDevToolsSession();

        if (_devTools.HasActiveDevToolsSession != true)
        {
            throw new InvalidOperationException("DevTools session could not be established.");
        }
    }

    public void SetNetworkCondtions(NetworkConditionsData networkConditions)
    {
        _session.Domains.Network.EnableNetwork();

        var command = new EmulateNetworkConditionsCommandSettings()
        {
            Offline = (bool)(networkConditions.Offline != null ? networkConditions.Offline : false)
        };

        if (networkConditions.ConnectionType != null)
        {
            command.ConnectionType = (ConnectionType)networkConditions.ConnectionType;
        }
        if (networkConditions.Latency != null)
        {
            command.Latency = (double)networkConditions.Latency;
        }
        if (networkConditions.DownloadThroughput != null)
        {
            command.DownloadThroughput = (double)networkConditions.DownloadThroughput;
        }
        if (networkConditions.UploadThroughput != null)
        {
            command.UploadThroughput = (double)networkConditions.UploadThroughput;
        }
        _session.SendCommand(command);
    }

    public void DisableNetworkEmulation()
    {
        _session.Domains.Network.DisableNetwork();
    }
}