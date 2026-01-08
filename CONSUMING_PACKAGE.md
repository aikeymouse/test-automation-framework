# Consuming the AIKeyMouse.Automation.Framework Package

This document explains how to install and use the `AIKeyMouse.Automation.Framework` NuGet package.

## Option 1: Download from GitHub Releases (Recommended - No Authentication Required)

This is the simplest method for public access.

### 1. Download the Package

1. Go to the [Releases page](https://github.com/aikeymouse/test-automation-framework/releases)
2. Find the latest release (e.g., v0.0.3)
3. Download the `.nupkg` file from the Assets section

### 2. Add to Your Project

There are several ways to use the downloaded package:

#### Method A: Local NuGet Source (Recommended)

1. Create a local packages folder in your solution:
   ```bash
   mkdir LocalPackages
   ```

2. Copy the downloaded `.nupkg` file to this folder

3. Create or update `nuget.config` in your solution root:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <packageSources>
       <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
       <add key="local" value="./LocalPackages" />
     </packageSources>
   </configuration>
   ```

4. Install the package:
   ```bash
   dotnet add package AIKeyMouse.Automation.Framework --version 0.0.3
   ```

#### Method B: Using Visual Studio

1. In Visual Studio, right-click your project → **Manage NuGet Packages**
2. Click the **gear icon** (Settings) in the top right
3. Click the **+** button to add a new package source
4. Set:
   - **Name**: `Local Packages`
   - **Source**: Browse to the folder containing your `.nupkg` file
5. Click **Update**, then **OK**
6. In the NuGet Package Manager, select **"Local Packages"** from the package source dropdown
7. Find **AIKeyMouse.Automation.Framework** and click **Install**

#### Method C: Direct Assembly Reference

1. Rename the `.nupkg` file to `.zip` and extract it
2. Navigate to `lib/net8.0/` in the extracted folder
3. Add a reference in your `.csproj`:
   ```xml
   <ItemGroup>
     <Reference Include="AIKeyMouse.Automation.Framework">
       <HintPath>path\to\extracted\lib\net8.0\AIKeyMouse.Automation.Framework.dll</HintPath>
     </Reference>
   </ItemGroup>
   ```

---

## Option 2: Install from GitHub Packages (Requires Authentication)

GitHub Packages requires authentication even for public packages.

### 1. Create a Personal Access Token (PAT)

1. **Create a Personal Access Token (PAT)**:
   - Go to https://github.com/settings/tokens
   - Click "Generate new token (classic)"
   - Select scopes: `read:packages`
   - Copy the generated token

### 2. Configure NuGet Authentication

Create or update your `nuget.config` file in your solution root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/aikeymouse/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_GITHUB_TOKEN" />
    </github>
  </packageSourceCredentials>
</configuration>
```

Replace:
- `YOUR_GITHUB_USERNAME` with your GitHub username
- `YOUR_GITHUB_TOKEN` with the token you created

**⚠️ Important**: Add `nuget.config` to `.gitignore` to avoid committing your token!

### 3. Install the Package

```bash
dotnet add package AIKeyMouse.Automation.Framework
```

Or in Visual Studio Package Manager Console:
```powershell
Install-Package AIKeyMouse.Automation.Framework
```

---

## Quick Start

After installation, you can start using the framework:

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
        
        // Windows automation (desktop apps)
        Window?.GetElement(LoginButton).Click();
    }
}
```

## Branches

- **main**: Stable version with FlaUI for Windows Desktop automation
- **desktop-automation**: Experimental version using native Windows UI Automation (CsWin32)

---

## Troubleshooting

### Downloaded Package Not Found

- Ensure the local package source path is correct in `nuget.config`
- Try using an absolute path instead of a relative path
- In Visual Studio, verify the package source is enabled in NuGet settings
- Clear NuGet cache: `dotnet nuget locals all --clear`

### "Unable to load the service index" Error (GitHub Packages)

- Verify your Personal Access Token is valid and has `read:packages` scope
- Check that your `nuget.config` file is in the solution root directory
- Ensure your GitHub username and token are correct

### Package Version Mismatch

- Check the actual version number in the downloaded `.nupkg` filename
- Specify the exact version when installing: `--version 0.0.3`
- Check available versions on the [Releases page](https://github.com/aikeymouse/test-automation-framework/releases)

### Visual Studio Not Showing Package

- Restart Visual Studio after adding the local package source
- Check that the package source is enabled (green checkmark in NuGet settings)
- Try switching between "All" and your local source in the package source dropdown

---

## Need Help?

Open an issue on the [GitHub repository](https://github.com/aikeymouse/test-automation-framework/issues).
