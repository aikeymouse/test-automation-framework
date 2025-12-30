using Reqnroll;
using AIKeyMouse.Automation.Framework.Infrastructure;
using AIKeyMouse.Automation.Framework.Helpers;
using AIKeyMouse.Automation.Framework.DataObjects;

namespace AIKeyMouse.Automation.Framework.StepDefinitions;

public class NetworkConditions
{
    public int Latency { get; set; }
    public int DownloadThroughput { get; set; }
    public int UploadThroughput { get; set; }
}

[Binding]
public class DevToolsSteps : StepsBase
{
    public DevToolsSteps(FeatureContext featureContext, ScenarioContext scenarioContext) 
        : base(featureContext, scenarioContext)
    {
    }

    [StepDefinition(@"I enable network emulation with the following parameters:")]
    public void EnableNetworkEmulation(Table table)
    {
        var networkConditions = table.CreateInstance<NetworkConditionsData>();

        var devToolsHelper = new DevToolsHelper(_driverContext);
        devToolsHelper.SetNetworkCondtions(networkConditions);
    }

    [StepDefinition(@"I disable network emulation")]
    public void DisableNetworkEmulation()
    {
        var devToolsHelper = new DevToolsHelper(_driverContext);
        devToolsHelper.DisableNetworkEmulation();
    }
}