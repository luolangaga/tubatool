using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;

namespace TubaWinUi3.Services;

public sealed record ToolDownloadInfo(
    string DownloadUrl,
    string FileName,
    long Size,
    bool IsArchive,
    bool IsInstaller);

public sealed record ToolDownloadProgress(
    long BytesReceived,
    long TotalBytes,
    double Percentage,
    double SpeedMbps,
    TimeSpan? EstimatedRemaining);

public static class ToolDownloaderService
{
    private const string HubBase = "https://hub.tubawinui3.cn";
    private const string BlenderListingUrl = "https://download.blender.org/release/BlenderBenchmark2.0/launcher/";

    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    static ToolDownloaderService()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TubaWinUi3-ToolDownloader");
    }

    public static async Task<ToolDownloadInfo?> ResolveDownloadUrlAsync(
        string downloadUrl, string? filter, CancellationToken ct = default)
    {
        if (downloadUrl.StartsWith("gh:", StringComparison.OrdinalIgnoreCase))
        {
            var repo = downloadUrl[3..];
            return await ResolveGitHubReleaseAsync(repo, filter, ct);
        }

        if (downloadUrl.Contains("blender.org", StringComparison.OrdinalIgnoreCase) &&
            downloadUrl.Contains("launcher", StringComparison.OrdinalIgnoreCase))
        {
            return await ResolveBlenderBenchmarkAsync(ct);
        }

        return null;
    }

    private static async Task<ToolDownloadInfo?> ResolveGitHubReleaseAsync(
        string repo, string? filter, CancellationToken ct)
    {
        var apiUrl = $"{HubBase}/api/repos/{repo}/releases/latest";

        try
        {
            var json = await _httpClient.GetStringAsync(apiUrl, ct);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("assets", out var assetsEl))
                return null;

            JsonElement bestAsset = default;
            var found = false;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                foreach (var asset in assetsEl.EnumerateArray())
                {
                    var name = asset.GetProperty("name").GetString() ?? "";
                    if (LikeMatch(name, filter))
                    {
                        bestAsset = asset;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                foreach (var asset in assetsEl.EnumerateArray())
                {
                    var name = asset.GetProperty("name").GetString() ?? "";
                    if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                        name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        bestAsset = asset;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
                return null;

            var assetName = bestAsset.GetProperty("name").GetString() ?? "";
            var assetUrl = bestAsset.GetProperty("browser_download_url").GetString() ?? "";
            var assetSize = bestAsset.TryGetProperty("size", out var sizeEl) ? sizeEl.GetInt64() : 0L;

            var isArchive = assetName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                            assetName.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase);
            var isInstaller = assetName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                              assetName.EndsWith(".msi", StringComparison.OrdinalIgnoreCase);

            var finalUrl = assetUrl.Replace("https://github.com", HubBase, StringComparison.OrdinalIgnoreCase);

            return new ToolDownloadInfo(finalUrl, assetName, assetSize, isArchive, isInstaller);
        }
        catch
        {
            return null;
        }
    }

    private static async Task<ToolDownloadInfo?> ResolveBlenderBenchmarkAsync(CancellationToken ct)
    {
        try
        {
            var html = await _httpClient.GetStringAsync(BlenderListingUrl, ct);

            var bestHref = "";
            var bestVersion = "";

            var idx = 0;
            while ((idx = html.IndexOf("benchmark-launcher-", idx, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                var hrefStart = html.IndexOf('"', idx);
                if (hrefStart < 0) break;
                var hrefEnd = html.IndexOf('"', hrefStart + 1);
                if (hrefEnd < 0) break;

                var href = html[(hrefStart + 1)..hrefEnd];
                idx = hrefEnd + 1;

                if (!href.EndsWith("-windows.zip", StringComparison.OrdinalIgnoreCase)) continue;
                if (href.Contains("-cli-", StringComparison.OrdinalIgnoreCase)) continue;

                var versionPart = ExtractVersion(href);
                if (string.IsNullOrEmpty(versionPart)) continue;

                if (string.Compare(versionPart, bestVersion, StringComparison.OrdinalIgnoreCase) > 0)
                {
                    bestVersion = versionPart;
                    bestHref = href;
                }
            }

            if (string.IsNullOrEmpty(bestHref))
                return null;

            var url = BlenderListingUrl + bestHref;
            var fileName = Path.GetFileName(bestHref);

            return new ToolDownloadInfo(url, fileName, 0, true, false);
        }
        catch
        {
            return null;
        }
    }

    public static async Task<string> DownloadToFileAsync(
        string url, string destinationDir, string fileName,
        IProgress<ToolDownloadProgress>? progress, CancellationToken ct = default)
    {
        Directory.CreateDirectory(destinationDir);
        var filePath = Path.Combine(destinationDir, fileName);
        if (File.Exists(filePath)) File.Delete(filePath);

        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
        var sw = Stopwatch.StartNew();

        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? 0;
        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var fs = File.Create(filePath);

        var buffer = new byte[81920];
        long bytesRead = 0;
        var lastReport = sw.Elapsed;
        long lastBytes = 0;

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var read = await stream.ReadAsync(buffer, ct);
            if (read == 0) break;

            await fs.WriteAsync(buffer.AsMemory(0, read), ct);
            bytesRead += read;

            var now = sw.Elapsed;
            if (now - lastReport > TimeSpan.FromMilliseconds(300))
            {
                var chunkBytes = bytesRead - lastBytes;
                var chunkTime = (now - lastReport).TotalSeconds;
                var speedMbps = chunkBytes / Math.Max(chunkTime, 0.001) * 8 / 1_000_000;
                var percentage = totalBytes > 0 ? (double)bytesRead / totalBytes * 100 : 0;
                var remaining = totalBytes > 0 && speedMbps > 0
                    ? TimeSpan.FromSeconds((totalBytes - bytesRead) / Math.Max(speedMbps * 1_000_000 / 8, 1))
                    : (TimeSpan?)null;

                progress?.Report(new ToolDownloadProgress(bytesRead, totalBytes, percentage, speedMbps, remaining));
                lastReport = now;
                lastBytes = bytesRead;
            }
        }

        progress?.Report(new ToolDownloadProgress(bytesRead, totalBytes, 100, 0, TimeSpan.Zero));
        return filePath;
    }

    public static async Task ExtractArchiveAsync(string archivePath, string destinationDir, CancellationToken ct = default)
    {
        await Task.Run(() =>
        {
            if (File.Exists(archivePath))
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(archivePath, destinationDir, true);
                File.Delete(archivePath);
            }
        }, ct);
    }

    private static bool LikeMatch(string input, string pattern)
    {
        var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*", ".*").Replace("\\?", ".") + "$";
        return System.Text.RegularExpressions.Regex.IsMatch(input, regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static string ExtractVersion(string href)
    {
        var match = System.Text.RegularExpressions.Regex.Match(href, @"(\d+\.\d+\.\d+)");
        return match.Success ? match.Groups[1].Value : "";
    }

    public static string FormatSize(long bytes)
    {
        if (bytes >= 1L << 30) return $"{(double)bytes / (1L << 30):F2} GB";
        if (bytes >= 1L << 20) return $"{(double)bytes / (1L << 20):F1} MB";
        if (bytes >= 1L << 10) return $"{(double)bytes / (1L << 10):F1} KB";
        return $"{bytes} B";
    }

    public static string FormatSpeed(double mbps)
    {
        if (mbps >= 1000) return $"{mbps / 1000:F2} Gbps";
        if (mbps >= 1) return $"{mbps:F2} Mbps";
        return $"{mbps * 1000:F0} Kbps";
    }

    public static string FormatTime(TimeSpan? time)
    {
        if (time is null || time.Value.TotalSeconds <= 0) return "--";
        var t = time.Value;
        if (t.TotalHours >= 1) return $"{(int)t.TotalHours}h {t.Minutes}m";
        if (t.TotalMinutes >= 1) return $"{t.Minutes}m {t.Seconds}s";
        return $"{t.Seconds}s";
    }
}
