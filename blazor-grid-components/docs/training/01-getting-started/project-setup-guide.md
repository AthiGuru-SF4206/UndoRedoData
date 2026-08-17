# Project Setup Guide — Syncfusion Blazor DataGrid

> **Audience**: New developers, freshers  
> **Module**: 01 — Getting Started  
> **Time Required**: 60–90 minutes  
> **Prerequisites**: [`architecture-overview.md`](./architecture-overview.md)  
> **Last Updated**: March 12, 2026

---

## Overview

This guide walks you through setting up a complete local development environment for contributing to the Syncfusion Blazor DataGrid component. Follow every step in order. Do not skip the verification steps.

---

## Prerequisites Checklist

Verify each item before proceeding:

| Requirement | Minimum Version | Verify Command |
|-------------|----------------|---------------|
| .NET SDK | 8.0 (LTS) or later | `dotnet --version` |
| Node.js | 18.x LTS | `node --version` |
| npm | 9.x | `npm --version` |
| Git | 2.40+ | `git --version` |
| Visual Studio 2022 | 17.8+ | Check Help → About |
| VS Code (alternative) | 1.85+ | `code --version` |

> **Recommended IDE**: Visual Studio 2022 with the ASP.NET and web development workload installed. VS Code with the C# Dev Kit extension is also supported.

---

## Step 1 — Clone the Repository

```bash
git clone https://dev.azure.com/EssentialStudio/Ej2-Web/_git/ej2-blazor-source
cd ej2-blazor-source
```

Verify the clone succeeded:
```bash
ls Syncfusion.Blazor/Grids/
# Expected: SfGrid.razor.cs, SfGrid.Properties.cs, Internal/, sf-grid.js, ...
```

---

## Step 2 — Restore NuGet Packages

```bash
dotnet restore Syncfusion.Blazor/Grids/Syncfusion.Blazor.Grid.csproj
```

Expected output: `Restore completed.` with no errors.

If you see `Unable to find package`, configure the Syncfusion NuGet feed:
```bash
dotnet nuget add source https://nuget.syncfusion.com/nuget_packages/v3/index.json \
  --name "Syncfusion" \
  --username "<your-email>" \
  --password "<your-api-key>"
```

Contact your team lead for the NuGet API key.

---

## Step 3 — Build the Grid Component

```bash
dotnet build Syncfusion.Blazor/Grids/Syncfusion.Blazor.Grid.csproj \
  --configuration Debug \
  --framework net8.0
```

**Expected**: `Build succeeded. 0 Warning(s). 0 Error(s).`

> **Zero analyzer warnings rule**: If any warning appears, do not proceed. Fix warnings before continuing. The grid enforces zero-warning builds in CI.

---

## Step 4 — Run a Sample Application

The repository includes sample applications for validating the grid. Navigate to a sample project:

```bash
cd samples/BlazorServerSample
dotnet run
```

Open the browser at `https://localhost:5001` and verify the grid renders with sample data.

**Checklist**:
- [ ] Grid renders rows and columns
- [ ] Sort works when clicking a column header
- [ ] Pager navigates between pages
- [ ] No console errors in the browser developer tools

---

## Step 5 — IDE Configuration

### Visual Studio 2022

1. Open `Syncfusion.Blazor.Grid.csproj` directly (not the solution)
2. Enable **XML documentation** warnings:
   - Project properties → Build → Generate documentation file: ✅
3. Enable **Nullable reference types**:
   - Verify `<Nullable>enable</Nullable>` exists in `.csproj`
4. Install the **Syncfusion Blazor Controls](https://marketplace.visualstudio.com/items?itemName=SyncfusionInc.SyncfusionBlazorVSExtensions)** VS extension for IntelliSense

### VS Code

Install the following extensions:
```
ms-dotnettools.csdevkit          (C# Dev Kit)
ms-dotnettools.csharp            (C# language support)
ms-dotnettools.vscode-dotnet-runtime
```

Add `.vscode/settings.json`:
```json
{
  "omnisharp.enableRoslynAnalyzers": true,
  "omnisharp.enableEditorConfigSupport": true,
  "editor.formatOnSave": true,
  "dotnet.defaultSolution": "Syncfusion.Blazor.Grid.csproj"
}
```

---

## Step 6 — Run Existing Tests

```bash
cd Syncfusion.Blazor/Grids
dotnet test --filter "Category=Grid" --configuration Debug
```

All existing tests must pass before you create your development branch.

If tests fail, do not proceed. Report the failure to the Scrum Master with the full test output.

---

## Step 7 — Create Your Development Branch

Follow the branching strategy from [`../../dev-process/branching-strategy.md`](../../dev-process/branching-strategy.md):

```bash
# For a new feature
git checkout develop
git pull origin develop
git checkout -b feature/your-feature-name

# For a bug fix
git checkout develop
git pull origin develop
git checkout -b bugfix/bug-id-short-description
```

Branch naming rules:
- Use lowercase with hyphens: `feature/add-column-menu-group`
- Include the work item ID for bug fixes: `bugfix/1015142-tab-after-grouping`
- Never commit directly to `develop` or `main`

---

## Folder Structure Reference

```
Syncfusion.Blazor/Grids/
│
├── SfGrid.razor.cs              ← Main component class
├── SfGrid.Properties.cs         ← All [Parameter] properties
├── SfGrid.Methods.cs            ← All public API methods
├── SfGrid.Lifecycle.cs          ← Lifecycle hooks
├── sf-grid.js                   ← JavaScript-side DOM operations
├── GridColumn.cs                ← GridColumn parameter model
├── GridEditSettings.cs          ← Edit settings parameter model
├── GridEvents.cs                ← EventCallback parameter declarations
├── Enumeration/
│   └── GridsEnumerations.cs     ← All public enums
├── EventModels/
│   └── Grids.cs                 ← All public event argument models
├── Interfaces/
│   └── IGrid.cs                 ← Public grid interface
└── Internal/
    ├── SfGrid.razor             ← Root render shell
    ├── Actions/                 ← 14 feature action modules
    ├── Base/                    ← Shared infrastructure
    ├── Editors/                 ← Edit mode renderers
    ├── Export/                  ← Excel, PDF, CSV export
    ├── Generators/              ← Column and row model generators
    ├── Models/                  ← Internal model types
    └── Renderer/                ← 30+ Razor render components
```

---

## Common Setup Issues

### Issue: `MSB3245` — Could not resolve assembly reference

**Cause**: Missing transitive Syncfusion package.  
**Fix**: Run `dotnet restore` from the solution root, not the individual project folder.

### Issue: Browser shows blank grid, no console errors

**Cause**: `sf-grid.js` not served. Check that Static Web Assets are enabled.  
**Fix**: Verify `<StaticWebAsset>` entries in `.csproj` and that `app.UseStaticFiles()` is in `Program.cs`.

### Issue: Analyzer warning `CS1591` (missing XML comment)

**Cause**: A `public` member is missing an XML documentation comment.  
**Fix**: Add the required `/// <summary>` comment. Do not suppress this warning with `#pragma`.

### Issue: Build fails with `CS8600` (possible null reference)

**Cause**: Nullable reference type violation.  
**Fix**: Add null check, use null-coalescing operator, or use the null-forgiving operator only when the null is impossible at runtime (with a comment explaining why).

---

## Verification Summary

Before starting any development work, verify all items:

- [ ] Repository cloned successfully
- [ ] `dotnet restore` completed without errors
- [ ] `dotnet build` completed with **zero warnings and zero errors**
- [ ] Sample application runs and grid renders correctly
- [ ] All existing tests pass
- [ ] Development branch created from `develop`
- [ ] IDE is configured with XML documentation and nullable reference types enabled

---

## Navigation

**Previous**: [`architecture-overview.md`](./architecture-overview.md)  
**Next**: [`../02-requirements-analysis/understanding-requirements.md`](../02-requirements-analysis/understanding-requirements.md)  
**Reference**: [`../../tech-stack/environment-setup.md`](../../tech-stack/environment-setup.md)
