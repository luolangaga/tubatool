using System.Diagnostics;
using System.Net.Http;

namespace TubaWinUi3.Services;

public static class SpeedTestService
{
    private static readonly HttpClient _downloadClient = new()
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    private static readonly HttpClient _uploadClient = new()
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    private static readonly TimeSpan MinTestDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan MaxTestDuration = TimeSpan.FromSeconds(45);

    private static readonly string CfDownloadUrl = "https://speed.cloudflare.com/__down?bytes=25000000";
    private static readonly string CfUploadUrl = "https://speed.cloudflare.com/__up";

    public static async Task<SpeedTestResult> RunDownloadTestAsync(IProgress<SpeedTestProgress>? progress, CancellationToken ct)
    {
        var totalBytes = 0L;
        var sw = Stopwatch.StartNew();
        var lastReport = 0L;
        var lastReportTime = TimeSpan.Zero;
        var speedSamples = new List<double>();
        var rounds = 0;

        try
        {
            while (sw.Elapsed < MinTestDuration)
            {
                ct.ThrowIfCancellationRequested();
                if (sw.Elapsed >= MaxTestDuration) break;

                var url = rounds < 2 ? CfDownloadUrl : (CfDownloadUrl + "&r=" + Guid.NewGuid());
                rounds++;

                using var response = await _downloadClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync(ct);
                var buffer = new byte[65536];

                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                    if (sw.Elapsed >= MaxTestDuration) break;

                    var read = await stream.ReadAsync(buffer, ct);
                    if (read == 0) break;

                    totalBytes += read;

                    var now = sw.Elapsed;
                    if (now - lastReportTime > TimeSpan.FromMilliseconds(250))
                    {
                        var chunkBytes = totalBytes - lastReport;
                        var chunkTime = now - lastReportTime;
                        var currentMbps = chunkBytes / chunkTime.TotalSeconds * 8 / 1_000_000;

                        speedSamples.Add(currentMbps);

                        progress?.Report(new SpeedTestProgress(
                            "下载",
                            totalBytes,
                            currentMbps,
                            sw.ElapsedMilliseconds
                        ));

                        lastReport = totalBytes;
                        lastReportTime = now;
                    }

                    if (sw.Elapsed >= MinTestDuration && totalBytes > 20_000_000)
                        break;
                }
            }

            sw.Stop();

            var avgMbps = ComputeAverage(speedSamples, totalBytes, sw.Elapsed.TotalSeconds);

            return new SpeedTestResult(true, avgMbps, 0, totalBytes, sw.ElapsedMilliseconds, "");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            var avgMbps = ComputeAverage(speedSamples, totalBytes, sw.Elapsed.TotalSeconds);
            return new SpeedTestResult(false, avgMbps, 0, totalBytes, sw.ElapsedMilliseconds, ex.Message);
        }
    }

    public static async Task<SpeedTestResult> RunUploadTestAsync(IProgress<SpeedTestProgress>? progress, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var totalBytes = 0L;
        var speedSamples = new List<double>();
        var lastReportTime = TimeSpan.Zero;
        var rounds = 0;

        try
        {
            while (sw.Elapsed < MinTestDuration)
            {
                ct.ThrowIfCancellationRequested();
                if (sw.Elapsed >= MaxTestDuration) break;

                var chunkSize = 2 * 1024 * 1024;
                var uploadData = new byte[chunkSize];
                new Random().NextBytes(uploadData);

                var content = new ByteArrayContent(uploadData);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                var chunkStart = sw.Elapsed;
                using var response = await _uploadClient.PostAsync(CfUploadUrl, content, ct);
                totalBytes += chunkSize;
                rounds++;

                var now = sw.Elapsed;
                var chunkTime = now - lastReportTime;
                if (chunkTime > TimeSpan.FromMilliseconds(100))
                {
                    var currentMbps = chunkSize / chunkTime.TotalSeconds * 8 / 1_000_000;
                    speedSamples.Add(currentMbps);

                    progress?.Report(new SpeedTestProgress(
                        "上传",
                        totalBytes,
                        currentMbps,
                        sw.ElapsedMilliseconds
                    ));

                    lastReportTime = now;
                }

                if (sw.Elapsed >= MinTestDuration)
                    break;
            }

            sw.Stop();

            var avgMbps = ComputeAverage(speedSamples, totalBytes, sw.Elapsed.TotalSeconds);

            return new SpeedTestResult(true, 0, avgMbps, totalBytes, sw.ElapsedMilliseconds, "");
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return new SpeedTestResult(false, 0, 0, 0, 0, $"上传测试暂不可用: {ex.Message}");
        }
    }

    private static double ComputeAverage(List<double> samples, long totalBytes, double totalSeconds)
    {
        if (samples.Count > 5)
            return samples.Skip(samples.Count / 10).Average();

        if (totalBytes > 0 && totalSeconds > 0)
            return totalBytes / totalSeconds * 8 / 1_000_000;

        return 0;
    }

    public static string FormatSpeed(double mbps)
    {
        if (mbps >= 1000)
            return $"{mbps / 1000:F2} Gbps";
        if (mbps >= 1)
            return $"{mbps:F2} Mbps";
        return $"{mbps * 1000:F0} Kbps";
    }
}

public sealed record SpeedTestResult(bool Success, double DownloadMbps, double UploadMbps, long Bytes, long ElapsedMs, string Error);
public sealed record SpeedTestProgress(string Phase, long BytesTransferred, double CurrentSpeedMbps, long ElapsedMs);