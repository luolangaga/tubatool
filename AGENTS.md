# TubaWinUi3 — Agent Notes

## What this is

A WinUI 3 (Windows App SDK) desktop app — a Chinese-language PC hardware toolbox ("图吧工具箱") that catalogs and launches third-party diagnostic/stress-test executables from a local `Tools/` folder, and displays WMI-based hardware info.

## Tech stack

- **.NET 10** (`net10.0-windows10.0.26100.0`), WinUI 3, MSIX-packaged
- Windows App SDK 1.8, `Microsoft.Windows.SDK.BuildTools.WinApp` for `dotnet run` support
- `System.Management` (WMI queries), `System.Drawing.Common` (icon extraction)
- No test framework, no solution file — single `.csproj`

## Build & run

```bash
dotnet build                          # default: Debug, x64 (RuntimeIdentifier auto-detects arch)
dotnet run                            # runs unpackaged (launchSettings "Unpackaged" profile)
dotnet run -p:Configuration=Debug     # same
```

- Packaged run requires MSIX registration; prefer the **Unpackaged** profile for dev.
- Platforms: x86, x64, ARM64 — RuntimeIdentifier defaults to current process architecture.

## Architecture

```
App.xaml.cs          → creates MainWindow
MainWindow.xaml.cs   → NavigationView with Frame; populates categories from ToolCatalog
Pages/
  HomePage           → tool grid, search, launch (Process.Start)
  HardwarePage       → WMI hardware info via HardwareInfoService
  SettingsPage       → placeholder
Services/
  ToolCatalog        → scans Tools/ for launchable files (.exe .bat .cmd .lnk .msc .ps1 .vbs)
  ToolMetadataService→ merges tools.json metadata + FileVersionInfo + readme.txt
  ToolIconService    → extracts .exe/.lnk icons to %LocalAppData%/TubaWinUi3/IconCache/
  HardwareInfoService→ WMI queries (Win32_ComputerSystem, Win32_Processor, etc.)
Models/
  ToolItem, HardwareInfoItem, HardwareInfoSection
Metadata/tools.json  → tool descriptions/publishers matched by filename substring
Tools/               → bundled third-party executables in Chinese-named category folders
```

## Key conventions

- **Namespace**: `TubaWinUi3` / `TubaWinUi3.Pages` / `TubaWinUi3.Services` / `TubaWinUi3.Models`
- **File naming**: PascalCase for all C# files; XAML + code-behind pairs
- **Services are static classes** with no DI — called directly from pages
- **UI strings are in Chinese** (hardcoded in XAML/C#); no resource localization system
- **`Tools/` content is bundled** via `<Content Include="Tools\**\*">` with `CopyToOutputDirectory=PreserveNewest`
- **`Metadata/tools.json`** uses `"match"` field for case-insensitive substring matching against tool filenames/paths

## Gotchas

- `Tools/` folder contains Chinese directory names (处理器工具, 显卡工具, etc.); path handling must be Unicode-safe
- `ToolCatalog.FindToolsRoot()` walks up from `AppContext.BaseDirectory` to find `Tools/` — works both packaged and unpackaged
- `HardwareInfoService` runs WMI on `Task.Run` (background thread); results are consumed on UI thread
- `ToolIconService` caches extracted icons as PNG in LocalApplicationData; cache key is SHA256 of the tool path
- `Package.appxmanifest` declares `runFullTrust` and `systemAIModels` capabilities
- No `.sln` file — all commands go through the `.csproj` directly
- Publish config: Release builds enable ReadyToRun + trimming; Debug disables both
