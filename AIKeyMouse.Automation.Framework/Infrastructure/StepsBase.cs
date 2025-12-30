using Reqnroll;

namespace AIKeyMouse.Automation.Framework.Infrastructure;

public partial class StepsBase(FeatureContext featureContext, ScenarioContext scenarioContext)
{
    protected readonly FeatureContext _featureContext = featureContext;
    protected readonly ScenarioContext _scenarioContext = scenarioContext;
    protected readonly DriverContext _driverContext = scenarioContext.Get<DriverContext>("DriverContext");
}