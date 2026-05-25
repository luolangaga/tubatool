using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace TubaWinUi3.Services;

public static class ToolIconService
{
    private static readonly string CacheRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TubaWinUi3",
        "IconCache");

    private static readonly TimeSpan CacheMaxAge = TimeSpan.FromDays(90);

    public static string? GetIconPath(string toolPath)
    {
        if (!File.Exists(toolPath))
        {
            return null;
        }

        var extension = Path.GetExtension(toolPath);
        if (!extension.Equals(".exe", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".lnk", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        Directory.CreateDirectory(CacheRoot);
        var iconPath = Path.Combine(CacheRoot, $"{Hash(toolPath)}.png");

        if (File.Exists(iconPath))
        {
            var age = DateTime.UtcNow - File.GetLastWriteTimeUtc(iconPath);
            if (age < CacheMaxAge)
                return iconPath;

            try { File.Delete(iconPath); } catch { return iconPath; }
        }

        try
        {
            using var icon = Icon.ExtractAssociatedIcon(toolPath);
            if (icon is null)
            {
                return null;
            }

            using var bitmap = icon.ToBitmap();
            bitmap.Save(iconPath, System.Drawing.Imaging.ImageFormat.Png);
            return iconPath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to extract icon for {toolPath}: {ex.Message}");
            return null;
        }
    }

    public static void CleanExpiredCache()
    {
        if (!Directory.Exists(CacheRoot))
            return;

        var cutoff = DateTime.UtcNow - CacheMaxAge;

        foreach (var file in Directory.EnumerateFiles(CacheRoot, "*.png"))
        {
            try
            {
                if (File.GetLastWriteTimeUtc(file) < cutoff)
                    File.Delete(file);
            }
            catch { }
        }
    }

    public static void CleanAllCache()
    {
        if (!Directory.Exists(CacheRoot))
            return;

        try
        {
            Directory.Delete(CacheRoot, true);
        }
        catch { }
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes)[..16].ToLowerInvariant();
    }
}
