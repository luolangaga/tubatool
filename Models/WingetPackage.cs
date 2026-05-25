namespace TubaWinUi3.Models;

public sealed class WingetPackage
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required string Glyph { get; init; }
    public string? Description { get; init; }
    public bool IsSelected { get; set; }
    public WingetInstallState State { get; set; } = WingetInstallState.NotInstalled;
    public string? StatusText { get; set; }
    public int Progress { get; set; }
}

public enum WingetInstallState
{
    NotInstalled,
    Checking,
    Installed,
    Installing,
    Succeeded,
    Failed,
    Skipped
}
