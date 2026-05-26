using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using TubaWinUi3.Models;

namespace TubaWinUi3.Services;

public static class UpdateService
{
    private const string Owner = "luolangaga";
    private const string Repo = "tubatool";
    private const string ReleaseApi = $"https://api.github.com/repos/{Owner}/{Repo}/releases/latest";
    private const string ReleasePage = $"https://github.com/{Owner}/{Repo}/releases/latest";

    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    private static string? _cachedEtag;
    private static string? _cachedJson;
    private static DateTime _lastCheckTime = DateTime.MinValue;

    private static readonly string[] ProxyList =
    [
        "https://ghfast.top",
        "https://gh-proxy.com",
        "https://ghproxy.net",
        "https://ghps.cc",
        "https://gh.idayer.com",
        "https://ghproxy.click",
        "https://mirror.ghproxy.com",
        "https://gh-proxy.com"
    ];

    public static string CurrentArchitecture { get; } = RuntimeInformation.OSArchitecture switch
    {
        Architecture.X64 => "x64",
        Architecture.Arm64 => "arm64",
        Architecture.X86 => "x86",
        _ => "x64"
    };

    static UpdateService()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TubaWinUi3-UpdateChecker");
    }

    public static Version CurrentVersion
    {
        get
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return v is not null ? new Version(v.Major, v.Minor, v.Build) : new Version(1, 0, 0);
        }
    }

    public static async Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken ct = default)
    {
        if (_cachedJson is not null && DateTime.Now - _lastCheckTime < TimeSpan.FromMinutes(10))
            return ParseUpdateJson(_cachedJson);

        var json = await FetchReleaseJsonAsync(ct);
        if (json is null) return null;

        _cachedJson = json;
        _lastCheckTime = DateTime.Now;

        return ParseUpdateJson(json);
    }

    private static async Task<string?> FetchReleaseJsonAsync(CancellationToken ct)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, ReleaseApi);
            if (_cachedEtag is not null)
                request.Headers.Add("If-None-Match", _cachedEtag);

            var response = await _httpClient.SendAsync(request, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
                return _cachedJson;

            if (response.IsSuccessStatusCode)
            {
                _cachedEtag = response.Headers.ETag?.Tag;
                return await response.Content.ReadAsStringAsync(ct);
            }
        }
        catch { }

        foreach (var proxy in ProxyList)
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
                client.DefaultRequestHeaders.Add("User-Agent", "TubaWinUi3-UpdateChecker");

                var proxyUrl = BuildProxyUrl(proxy, ReleaseApi);
                var response = await client.GetAsync(proxyUrl, ct);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync(ct);
            }
            catch { }
        }

        return null;
    }

    private static UpdateInfo? ParseUpdateJson(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var tagName = root.GetProperty("tag_name").GetString() ?? "";
            var versionStr = tagName.TrimStart('v', 'V');

            if (!Version.TryParse(versionStr, out var remoteVersion))
                return null;

            if (remoteVersion <= CurrentVersion)
                return null;

            var assets = new List<UpdateAsset>();
            if (root.TryGetProperty("assets", out var assetsEl))
            {
                foreach (var asset in assetsEl.EnumerateArray())
                {
                    var name = asset.GetProperty("name").GetString() ?? "";
                    var url = asset.GetProperty("browser_download_url").GetString() ?? "";
                    var size = asset.TryGetProperty("size", out var sizeEl) ? sizeEl.GetInt64() : 0;
                    var contentType = asset.TryGetProperty("content_type", out var ctEl) ? ctEl.GetString() : null;

                    if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                        name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                        name.EndsWith(".msix", StringComparison.OrdinalIgnoreCase) ||
                        name.EndsWith(".msixbundle", StringComparison.OrdinalIgnoreCase))
                    {
                        assets.Add(new UpdateAsset
                        {
                            Name = name,
                            BrowserDownloadUrl = url,
                            Size = size,
                            ContentType = contentType
                        });
                    }
                }
            }

            return new UpdateInfo
            {
                Version = versionStr,
                HtmlUrl = root.GetProperty("html_url").GetString() ?? "",
                Body = root.TryGetProperty("body", out var body) ? body.GetString() : null,
                PublishedAt = root.GetProperty("published_at").GetDateTimeOffset(),
                Assets = assets,
                IsPrerelease = root.TryGetProperty("prerelease", out var pre) && pre.GetBoolean()
            };
        }
        catch
        {
            return null;
        }
    }

    public static async Task<List<ProxySpeedResult>> TestProxySpeedsAsync(
        string originalUrl, IProgress<ProxySpeedResult>? progress = null, CancellationToken ct = default)
    {
        var probeUrl = "https://github.com/favicon.ico";
        var tasks = ProxyList.Select(proxy => TestSingleProxy(proxy, probeUrl, ct)).ToList();

        var results = new List<ProxySpeedResult>();

        while (tasks.Count > 0)
        {
            var finished = await Task.WhenAny(tasks);
            tasks.Remove(finished);

            try
            {
                var result = await finished;
                results.Add(result);
                progress?.Report(result);
            }
            catch { }
        }

        return results
            .Where(r => r.IsAvailable)
            .OrderBy(r => r.LatencyMs)
            .ToList();
    }

    private static async Task<ProxySpeedResult> TestSingleProxy(
        string proxyBase, string probeUrl, CancellationToken ct)
    {
        var proxyUrl = BuildProxyUrl(proxyBase, probeUrl);
        var name = new Uri(proxyBase).Host;

        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };

            var sw = Stopwatch.StartNew();
            using var response = await client.GetAsync(proxyUrl, ct);
            sw.Stop();

            if (!response.IsSuccessStatusCode)
            {
                return new ProxySpeedResult
                {
                    Name = name, BaseUrl = proxyBase, LatencyMs = sw.Elapsed.TotalMilliseconds,
                    SpeedMbps = 0, IsAvailable = false, Error = $"HTTP {(int)response.StatusCode}"
                };
            }

            return new ProxySpeedResult
            {
                Name = name, BaseUrl = proxyBase, LatencyMs = sw.Elapsed.TotalMilliseconds,
                SpeedMbps = 0, IsAvailable = true
            };
        }
        catch (Exception ex)
        {
            return new ProxySpeedResult
            {
                Name = name, BaseUrl = proxyBase, LatencyMs = double.MaxValue,
                SpeedMbps = 0, IsAvailable = false, Error = ex.Message
            };
        }
    }

    public static async Task<string> DownloadUpdateAsync(
        UpdateAsset asset, string? proxyBaseUrl, IProgress<DownloadProgress>? progress,
        CancellationToken ct = default)
    {
        var downloadUrl = proxyBaseUrl is not null
            ? BuildProxyUrl(proxyBaseUrl, asset.BrowserDownloadUrl)
            : asset.BrowserDownloadUrl;

        var tempDir = Path.Combine(Path.GetTempPath(), "TubaWinUi3_Update");
        Directory.CreateDirectory(tempDir);

        var filePath = Path.Combine(tempDir, asset.Name);
        if (File.Exists(filePath))
            File.Delete(filePath);

        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
        var sw = Stopwatch.StartNew();

        using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? asset.Size;
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

                progress?.Report(new DownloadProgress
                {
                    BytesReceived = bytesRead,
                    TotalBytes = totalBytes,
                    Percentage = percentage,
                    SpeedMbps = speedMbps,
                    Elapsed = now,
                    EstimatedRemaining = remaining
                });

                lastReport = now;
                lastBytes = bytesRead;
            }
        }

        progress?.Report(new DownloadProgress
        {
            BytesReceived = bytesRead,
            TotalBytes = totalBytes,
            Percentage = 100,
            SpeedMbps = 0,
            Elapsed = sw.Elapsed,
            EstimatedRemaining = TimeSpan.Zero
        });

        return filePath;
    }

    public static string BuildProxyUrl(string proxyBase, string originalUrl)
    {
        proxyBase = proxyBase.TrimEnd('/');
        return $"{proxyBase}/{originalUrl}";
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

    public static UpdateAsset? FindBestAsset(List<UpdateAsset> assets)
    {
        var arch = CurrentArchitecture;

        var match = assets.FirstOrDefault(a =>
            a.Name.Contains(arch, StringComparison.OrdinalIgnoreCase) &&
            a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

        if (match is not null) return match;

        match = assets.FirstOrDefault(a =>
            a.Name.Contains(arch, StringComparison.OrdinalIgnoreCase) &&
            (a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
             a.Name.EndsWith(".msixbundle", StringComparison.OrdinalIgnoreCase) ||
             a.Name.EndsWith(".msix", StringComparison.OrdinalIgnoreCase)));

        if (match is not null) return match;

        match = assets.FirstOrDefault(a =>
            a.Name.Contains(arch, StringComparison.OrdinalIgnoreCase));

        return match;
    }
}
