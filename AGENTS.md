# TubaWinUi3 — Agent Notes

## What this is

A WinUI 3 (Windows App SDK) desktop app — a Chinese-language PC hardware toolbox ("图吧工具箱") that catalogs and launches third-party diagnostic/stress-test executables from a local `Tools/` folder, displays WMI/LibreHardwareMonitor hardware info, and provides built-in utility tools.

## Build & run

```bash
dotnet build                          # Debug, x64 (RuntimeIdentifier auto-detects arch)
dotnet run                            # Unpackaged profile (the only profile in launchSettings.json)
```

- **No `.sln` file** — all commands go through `TubaWinUi3.csproj`
- `EnableMsixTooling=false` + `WindowsPackageType=None` — app runs unpackaged; no MSIX registration needed for dev
- Platforms: x86, x64, ARM64 — RuntimeIdentifier defaults to current process architecture
- No test framework configured — verify with `dotnet build` only

## Architecture

```
App.xaml.cs          → creates MainWindow
MainWindow.xaml.cs   → TitleBar + NavigationView with Frame; populates nav categories from ToolCatalog
Pages/
  HomePage           → tool grid, search, launch (Process.Start), admin-elevate, desktop shortcut
  FavoritesPage      → persisted favorites
  HardwarePage       → WMI hardware info via HardwareInfoService
  BuiltinToolsPage   → built-in tools from BuiltinToolRegistry
  LiteMonitorPage    → real-time hardware monitor (LibreHardwareMonitor + kernel driver)
  SettingsPage       → theme, update, compact mode
  ToolDetailDialog, ToolDownloadDialog, UpdateDialog, ErrorPage
Services/
  ToolCatalog        → scans Tools/ for launchable files (.exe .bat .cmd .lnk .msc .ps1 .vbs)
                      merges arch variants (x64/x86 dirs), caches tags/search
  ToolMetadataService→ merges tools.json metadata + FileVersionInfo + readme.txt
  ToolIconService    → extracts .exe/.lnk icons to %LocalAppData%/TubaWinUi3/IconCache/ (SHA256 cache key)
  HardwareInfoService→ WMI queries (Win32_ComputerSystem, Win32_Processor, etc.) on Task.Run
  LiteMonitorService → LibreHardwareMonitor + WinRing0 kernel driver deploy; requires admin
  BuiltinToolRegistry→ static registry of IBuiltinTool implementations
  IBuiltinTool       → interface: Id, Name, Description, Glyph, Category, Kind, ExecuteAsync()
                      Kinds: Dialog, BackgroundTask, ProgressTask, InstantAction
  BuiltinTools/      → 15 built-in tools (PowerMonitor, CertBlock, PortViewer, HostsEditor,
                      KeyboardTest, JunkCleaner, BsodAnalysis, WingetInstaller, BatteryReport,
                      SpeedTest, WifiPassword, DiskSpaceAnalyzer, LiteMonitor, WindowsActivation, Defender)
  AppSettings        → JSON settings at %LocalAppData%/TubaWinUi3/settings.json
  ThemeService, WindowSizeService, FavoritesService, UpdateService, etc.
Models/
  ToolItem, HardwareInfoItem, HardwareInfoSection, ArchVariant
Metadata/tools.json  → tool descriptions/publishers/tags matched by "match" field (case-insensitive substring)
                      supports "archVariants" with "file"/"dir" + "arch" per variant
Tools/               → bundled third-party executables in Chinese-named category folders
CertBlock/           → certificate blocking data + assets
remotedefender/      → included in publish output (verified by CI)
src/docs/            → VitePress website (tubawinui3.cn), not part of the .NET build
```

## Adding a built-in tool

1. Create a new class in `Services/BuiltinTools/` implementing `IBuiltinTool`
2. Choose `BuiltinToolKind`: `Dialog` (popup UI), `BackgroundTask` (run silently), `ProgressTask` (progress bar), `InstantAction` (immediate)
3. Register in `BuiltinToolRegistry.RegisterDefaults()` — duplicate IDs throw
4. Use `context.CreateDialog(title)` to create themed ContentDialogs

## Key conventions

- **Namespace**: `TubaWinUi3` / `TubaWinUi3.Pages` / `TubaWinUi3.Services` / `TubaWinUi3.Models`
- **Services are static classes** with no DI — called directly from pages
- **UI strings are in Chinese** (hardcoded in XAML/C#); no resource localization system
- **`Tools/` content is bundled** via `<Content Include="Tools\**\*">` with `CopyToOutputDirectory=PreserveNewest`
- **`Metadata/tools.json`** `"match"` field: case-insensitive substring match against tool filenames/paths
- **File naming**: PascalCase for C#; XAML + code-behind pairs
- **Commit format**: `feat:` / `fix:` / `docs:` / `refactor:` (from README)
- **Never commit**: `bin/`, `obj/`, `.pfx`, `.cer`

## Gotchas

- `Tools/` folder has Chinese directory names (处理器工具, 显卡工具, etc.); path handling must be Unicode-safe
- `ToolCatalog.FindToolsRoot()` walks up from `AppContext.BaseDirectory` to find `Tools/` — works both packaged and unpackaged
- `HardwareInfoService` runs WMI on `Task.Run` (background thread); results consumed on UI thread
- `LiteMonitorService` deploys WinRing0 driver (`WinRing0x64.sys`) — requires admin elevation; the `EnsureDriverAsync` flow handles consent UI
- `Package.appxmanifest` declares `runFullTrust` and `systemAIModels` capabilities
- Publish config: `PublishTrimmed=false`, `PublishReadyToRun=false` in both Debug and Release — trimming is not used
- `.pri` file must be manually restored from `bin/` to publish output after `dotnet publish` (both CI and build scripts do this)
- `package.json` / `node_modules/` / `src/docs/` are for the VitePress docs site only — not part of the .NET app
- `build-setup.ps1` builds Inno Setup installer (x64 + ARM64); `build-store.ps1` builds MSIX for Store submission
- CI workflow (`.github/workflows/build-release.yml`) is manual dispatch only (`workflow_dispatch`), publishes x64/x86/ARM64 portable zip + Inno installer, generates changelog via DeepSeek API

## Website (docs)

```bash
npm run dev       # VitePress dev server at src/docs
npm run build     # Build static site
```

Separate from the .NET build — `node_modules/` and `src/` are not referenced by the `.csproj`.
