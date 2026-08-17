# Environment Setup — Syncfusion Blazor DataGrid

> **Audience**: New developers, contributors, and CI/CD engineers
> **Prerequisite**: [`tech-stack/tech-stack.md`](./tech-stack.md)
> **Last Updated**: March 11, 2026

---

## Overview

This guide walks through the complete environment setup to build, run, test, and contribute to the `Syncfusion.Blazor.Grid` component project on Windows and macOS.

---

## 1. Prerequisites Checklist

Before cloning the repository, verify the following tools are installed:

### Required

| Tool | Minimum Version | Verify With | Download |
|------|-----------------|-------------|----------|
| **.NET SDK** | 8.0.x, 9.0.x, or 10.0.x | `dotnet --version` | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| **Git** | 2.40+ | `git --version` | [git-scm.com](https://git-scm.com) |
| **Visual Studio 2022** | 17.8+ (Windows) | VS Installer | [visualstudio.microsoft.com](https://visualstudio.microsoft.com) |
| **OR VS Code** | 1.85+ (Windows/macOS) | `code --version` | [code.visualstudio.com](https://code.visualstudio.com) |

### Recommended VS Code Extensions

| Extension | ID | Purpose |
|-----------|----|---------|
| C# Dev Kit | `ms-dotnettools.csdevkit` | Full .NET development support |
| Blazor WASM Debugging | `ms-dotnettools.blazorwasm-companion` | WASM breakpoint debugging |
| GitLens | `eamodio.gitlens` | Advanced Git history and blame |
| EditorConfig | `editorconfig.editorconfig` | Enforce project coding style |

### Required .NET Workloads

Run once after SDK installation:

```powershell
dotnet workload install wasm-tools
dotnet workload install aspnet
```

---

## 2. Repository Setup

### Step 1 — Clone the Repository

```powershell
git clone https://gitea.internal.syncfusion.com/blazor/ej2-blazor-source.git
cd ej2-blazor-source
```

### Step 2 — Navigate to the Grid Component

```powershell
cd Syncfusion.Blazor\Grids
```

### Step 3 — Restore Dependencies

```powershell
dotnet restore
```

> ✅ Expected: All 11 Syncfusion package references resolve from the internal NuGet feed or NuGet.org.

### Step 4 — Configure NuGet Feed (Internal Builds)

If pre-release packages are required, add the internal Syncfusion NuGet feed:

```powershell
dotnet nuget add source https://nuget.internal.syncfusion.com/v3/index.json `
  --name SyncfusionInternal `
  --username <your-username> `
  --password <your-token>
```

---

## 3. Build Commands

| Command | Purpose | Output |
|---------|---------|--------|
| `dotnet build` | Debug build | `bin/Debug/net8.0/` |
| `dotnet build -c Release` | Release build | `bin/Release/net*/` |
| `dotnet build -f net10.0` | Target a specific framework | Single-TFM output |
| `dotnet pack -c Release` | Create NuGet package | `bin/Release/*.nupkg` |
| `dotnet clean` | Remove all build artifacts | Clears `bin/` and `obj/` |

### Multi-Target Build Verification

The project targets `net8.0`, `net9.0`, and `net10.0`. Always verify a clean build across all targets:

```powershell
dotnet build -c Release /p:TreatWarningsAsErrors=true
```

> ✅ Expected: Zero errors, zero warnings across all three TFMs.

---

## 4. Running Tests

### Unit Tests (bUnit)

```powershell
# From repository root
dotnet test Syncfusion.Blazor.Grid.Tests/ --logger "console;verbosity=normal"
```

### Run a specific test class

```powershell
dotnet test --filter "ClassName=SfGrid_Selection_Tests"
```

### Run with code coverage

```powershell
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

### E2E Tests (Playwright)

```powershell
# Install Playwright browsers (first-time only)
pwsh Syncfusion.Blazor.Grid.E2E/bin/Debug/net8.0/playwright.ps1 install

# Run E2E tests
dotnet test Syncfusion.Blazor.Grid.E2E/
```

---

## 5. IDE Configuration

### Visual Studio 2022

1. Open `Syncfusion.Blazor.Grid.csproj` directly (or the parent `.sln` if available)
2. Set target framework to `net8.0` for fastest hot reload
3. Enable **Nullable Reference Types** warnings in Tools → Options → Text Editor → C# → Code Style

**Recommended settings** (`Tools → Options`):

| Setting | Value |
|---------|-------|
| Build → Treat Warnings as Errors | Release builds only |
| C# Formatting → Indentation | 4 spaces |
| C# Naming → Private fields | `_camelCase` |

### VS Code

1. Open the `Grids/` folder: `code d:\Gitea\BlazorSource\development\ej2-blazor-source\Syncfusion.Blazor\Grids`
2. Accept the recommended extensions prompt
3. Select `.NET SDK` version when prompted by C# Dev Kit

**`.vscode/settings.json`** (create if absent):

```json
{
  "dotnet.defaultSolution": "Syncfusion.Blazor.Grid.csproj",
  "editor.formatOnSave": true,
  "editor.tabSize": 4,
  "files.trimTrailingWhitespace": true,
  "csharp.format.enable": true
}
```

---

## 6. EditorConfig

The project ships an `.editorconfig` at the repository root. Key rules:

```ini
root = true

[*.cs]
indent_style = space
indent_size = 4
end_of_line = crlf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

[*.razor]
indent_style = space
indent_size = 4
```

Do not override `.editorconfig` locally — it enforces the team's code style uniformly.

---

## 7. Debugging

### Blazor Server (Local)

1. Create a test Blazor Server host project in the `samples/` directory
2. Add a `<ProjectReference>` to `Syncfusion.Blazor.Grid.csproj`
3. Press **F5** in Visual Studio — standard .NET breakpoints work

### Blazor WASM

1. Use `dotnet watch` in the host project for hot reload
2. Attach the Blazor WASM debugger from the VS Code Run panel
3. Set `DOTNET_ENVIRONMENT=Development` for full exception detail

### JS Interop Debugging

1. Open Browser DevTools → Sources
2. Locate `_content/Syncfusion.Blazor.Grid/sf-grid.js` (served unminified in Debug mode)
3. Set breakpoints on `window.sfBlazor.Grid.*` functions

---

## 8. Environment Variables

| Variable | Purpose | Example |
|----------|---------|---------|
| `DOTNET_ENVIRONMENT` | Sets the application environment | `Development` |
| `SYNCFUSION_LICENSE_KEY` | Required to suppress license warning in test host | `<your-license-key>` |
| `NUGET_PACKAGES` | Override NuGet global packages cache | `D:\NuGetCache` |

Set in PowerShell:

```powershell
$env:SYNCFUSION_LICENSE_KEY = "your-key-here"
$env:DOTNET_ENVIRONMENT = "Development"
```

---

## 9. Troubleshooting

### Problem: `dotnet restore` fails with 401 Unauthorized

**Cause**: Internal NuGet feed credentials expired.

**Fix**:
```powershell
dotnet nuget update source SyncfusionInternal --username <user> --password <new-token>
```

---

### Problem: Build fails with `CS8618: Non-nullable field must contain a non-null value`

**Cause**: Nullable reference types are enabled. A field lacks initialization.

**Fix**: Initialize the field or suppress with `= null!` only if the lifecycle guarantees initialization before use.

---

### Problem: Razor component changes not reflected in hot reload

**Cause**: Multi-TFM projects have known hot reload limitations.

**Fix**: Set a single target framework during development:
```powershell
dotnet watch --framework net8.0
```

---

### Problem: Assembly strong-name validation failure at runtime

**Cause**: `sf.snk` key file is missing or the assembly was built without signing.

**Fix**: Ensure `sf.snk` is present at `Syncfusion.Blazor/../sf.snk`. Do not generate a new key — obtain the canonical key from the repository.

---

### Problem: JS interop throws `JSException: Cannot read properties of undefined`

**Cause**: Grid JS module not yet initialized when the C# code calls interop.

**Fix**: Check that `await base.OnAfterRenderAsync(firstRender)` is called before any `InvokeMethod` in custom lifecycle overrides.

---

### Problem: Playwright tests fail with `TimeoutError`

**Cause**: Grid not finishing data load before assertion.

**Fix**: Use `await page.WaitForSelector('.e-grid .e-row')` instead of fixed delays.

---

## 10. CI/CD Integration Points

| Stage | Command | Gate |
|-------|---------|------|
| **Build** | `dotnet build -c Release /p:TreatWarningsAsErrors=true` | Zero errors/warnings |
| **Unit Tests** | `dotnet test --collect:"XPlat Code Coverage"` | All tests pass |
| **Pack** | `dotnet pack -c Release` | Package created without warnings |
| **E2E** | `dotnet test Syncfusion.Blazor.Grid.E2E/` | Zero failures |

Branch protection on `main` and `develop` requires all four stages to pass before merge.

---

*For language and framework details, see [`tech-stack/tech-stack.md`](./tech-stack.md).*
*For package dependencies, see [`tech-stack/third-party-libraries.md`](./third-party-libraries.md).*
