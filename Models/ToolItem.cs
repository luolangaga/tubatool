namespace TubaWinUi3.Models;

public sealed class ToolItem
{
    public required string Name { get; init; }

    public required string Category { get; init; }

    public required string Path { get; init; }

    public required string RelativePath { get; init; }

    public required string Extension { get; init; }

    public string? IconPath { get; init; }

    public string? IconGlyph { get; init; }

    public string? Description { get; init; }

    public string? Publisher { get; init; }

    public string? Version { get; init; }

    public string? DatabaseSource { get; init; }

    public string? DownloadUrl { get; init; }

    public string? DownloadFilter { get; init; }

    public bool IsFavorite { get; set; }

    public string Folder => System.IO.Path.GetDirectoryName(RelativePath) ?? Category;

    public bool NeedsDownload => !string.IsNullOrWhiteSpace(DownloadUrl);

    public string? PrimaryArch { get; init; }

    public IReadOnlyList<ArchVariant> AlternateVersions { get; init; } = [];

    public bool HasAlternateVersions => AlternateVersions.Count > 0;

    public string LaunchButtonText
    {
        get
        {
            if (NeedsDownload)
                return "下载";
            if (PrimaryArch is not null)
                return $"打开（{PrimaryArch}）";
            return "打开";
        }
    }
}

public sealed class ArchVariant
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required string Arch { get; init; }
}
