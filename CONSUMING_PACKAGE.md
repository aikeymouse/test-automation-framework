# How to Use AIKeyMouse.Automation.Framework from GitHub Packages

This framework is published to GitHub Packages and can be installed in your .NET projects.

## Prerequisites

- .NET 8.0 SDK or later
- GitHub account

## Setup Authentication

### Option 1: Using GitHub Personal Access Token (Recommended)

1. **Create a Personal Access Token (PAT)**:
   - Go to https://github.com/settings/tokens
   - Click "Generate new token (classic)"
   - Select scopes: `read:packages`
   - Copy the generated token

2. **Configure NuGet source**:

```bash
dotnet nuget add source https://nuget.pkg.github.com/aikeymouse/index.json \
  --name github-aikeymouse \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_TOKEN \
  --store-password-in-clear-text
```

### Option 2: Using nuget.config in Your Project

Create a `nuget.config` file in your project root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="github-aikeymouse" value="https://nuget.pkg.github.com/aikeymouse/index.json" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
  <packageSourceCredentials>
    <github-aikeymouse>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_GITHUB_TOKEN" />
    </github-aikeymouse>
  </packageSourceCredentials>
</configuration>
```

**⚠️ Important**: Add `nuget.config` to `.gitignore` to avoid committing your token!

## Install the Package

Once configured, install the package in your test project:

```bash
dotnet add package AIKeyMouse.Automation.Framework --version 0.0.1
```

Or add to your `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="AIKeyMouse.Automation.Framework" Version="0.0.1" />
</ItemGroup>
```

## Quick Start Example

```csharp
using AIKeyMouse.Automation.Framework.Infrastructure;
using AIKeyMouse.Automation.Framework.DataObjects;
using AIKeyMouse.Automation.Framework.Extensions;

public class MyTestPage : PageBase
{
    private ElementLocator LoginButton => new(Locator.Id, "loginBtn");
    
    public void ClickLogin()
    {
        // Web automation
        WebDriver?.GetElement(LoginButton).Click();
        
        // Mobile automation
        Driver?.GetElement(LoginButton).Click();
        
        // Windows automation
        Window?.GetElement(LoginButton).Click();
    }
}
```

## Branches

- **main**: Stable version with FlaUI for Windows Desktop automation
- **desktop-automation**: Experimental version using native Windows UI Automation (CsWin32)

To use a specific branch build, check the [Packages](https://github.com/aikeymouse/test-automation-framework/packages) page for available versions.

## Troubleshooting

### "Unable to load the service index" error

Make sure you've authenticated with GitHub Packages using one of the methods above.

### "Package not found" error

Verify:
1. The package source is correctly added: `dotnet nuget list source`
2. You have access to the repository
3. The package version exists

### Need Help?

Open an issue on the [GitHub repository](https://github.com/aikeymouse/test-automation-framework/issues).
