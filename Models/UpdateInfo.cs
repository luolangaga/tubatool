namespace TubaWinUi3.Models;

public sealed class UpdateInfo
{
    public required string Version { get; init; }
    public required string HtmlUrl { get; init; }
    public string? Body { get; init; }
    public required DateTimeOffset PublishedAt { get; init; }
    public required List<UpdateAsset> Assets { get; init; }
    public bool IsPrerelease { get; init; }
}

public sealed class UpdateAsset
{
    public required string Name { get; init; }
    public required string BrowserDownloadUrl { get; init; }
    public long Size { get; init; }
    public string? ContentType { get; init; }
}

public sealed class ProxySpeedResult
{
    public required string Name { get; init; }
    public required string BaseUrl { get; init; }
    public required double LatencyMs { get; init; }
    public required double SpeedMbps { get; init; }
    public bool IsAvailable { get; init; }
    public string? Error { get; init; }
}

public sealed class DownloadProgress
{
    public required long BytesReceived { get; init; }
    public required long TotalBytes { get; init; }
    public required double Percentage { get; init; }
    public required double SpeedMbps { get; init; }
    public required TimeSpan Elapsed { get; init; }
    public required TimeSpan? EstimatedRemaining { get; init; }
}

public enum UpdateState
{
    Checking,
    UpdateAvailable,
    NoUpdate,
    TestingProxies,
    Downloading,
    ReadyToInstall,
    Error
}
