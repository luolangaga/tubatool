namespace TubaWinUi3.Models;

public sealed class ToolItem
{
    public required string Name { get; init; }

    public required string Category { get; init; }

    public required string Path { get; init; }

    public required string RelativePath { get; init; }

    public required string Extension { get; init; }

    public string? IconPath { get; init; }

    public string? Description { get; init; }

    public string? Publisher { get; init; }

    public string? Version { get; init; }

    public string? DatabaseSource { get; init; }

    public string? DownloadUrl { get; init; }

    public string? DownloadFilter { get; init; }

    public bool IsFavorite { get; set; }

    public string Folder => System.IO.Path.GetDirectoryName(RelativePath) ?? Category;

    public bool NeedsDownload => !string.IsNullOrWhiteSpace(DownloadUrl);

    public string LaunchButtonText => NeedsDownload ? "下载" : "打开";
}
