using System.Diagnostics;

namespace TubaWinUi3.Services;

public static class WifiPasswordService
{
    public static async Task<List<WifiNetwork>> GetNetworksAsync()
    {
        var networks = new List<WifiNetwork>();

        try
        {
            var profiles = await RunNetshAsync("wlan show profiles");
            var profileNames = new List<string>();

            foreach (var line in profiles.Split('\n'))
            {
                var trimmed = line.Trim();
                var colonIdx = trimmed.IndexOf(':');
                if (colonIdx > 0 && trimmed.Contains("所有用户配置文件") || trimmed.Contains("All User Profile"))
                {
                    var name = trimmed[(colonIdx + 1)..].Trim();
                    if (!string.IsNullOrEmpty(name))
                        profileNames.Add(name);
                }
            }

            foreach (var name in profileNames)
            {
                var network = new WifiNetwork { Ssid = name };
                try
                {
                    var detail = await RunNetshAsync($"wlan show profile name=\"{name}\" key=clear");
                    foreach (var line in detail.Split('\n'))
                    {
                        var trimmed = line.Trim();
                        if (trimmed.Contains("关键内容") || trimmed.Contains("Key Content"))
                        {
                            var colonIdx = trimmed.IndexOf(':');
                            if (colonIdx > 0)
                            {
                                network.Password = trimmed[(colonIdx + 1)..].Trim();
                                network.HasPassword = true;
                            }
                        }
                        if (trimmed.Contains("身份验证") || trimmed.Contains("Authentication"))
                        {
                            var colonIdx = trimmed.IndexOf(':');
                            if (colonIdx > 0)
                                network.Authentication = trimmed[(colonIdx + 1)..].Trim();
                        }
                        if (trimmed.Contains("加密") || trimmed.Contains("Cipher"))
                        {
                            var colonIdx = trimmed.IndexOf(':');
                            if (colonIdx > 0)
                                network.Cipher = trimmed[(colonIdx + 1)..].Trim();
                        }
                    }
                }
                catch { }

                networks.Add(network);
            }
        }
        catch { }

        return networks;
    }

    public static async Task<WifiNetwork?> GetCurrentNetworkAsync()
    {
        try
        {
            var output = await RunNetshAsync("wlan show interfaces");
            string? ssid = null;

            foreach (var line in output.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.Contains("SSID") && trimmed.Contains(':') && !trimmed.Contains("BSSID"))
                {
                    var colonIdx = trimmed.IndexOf(':');
                    ssid = trimmed[(colonIdx + 1)..].Trim();
                }
            }

            if (string.IsNullOrEmpty(ssid)) return null;
            return new WifiNetwork { Ssid = ssid, IsConnected = true };
        }
        catch
        {
            return null;
        }
    }

    private static async Task<string> RunNetshAsync(string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = args,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8
        };

        using var process = Process.Start(psi);
        if (process is null) return "";

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return output;
    }
}

public sealed class WifiNetwork
{
    public string Ssid { get; set; } = "";
    public string Password { get; set; } = "";
    public bool HasPassword { get; set; }
    public string Authentication { get; set; } = "";
    public string Cipher { get; set; } = "";
    public bool IsConnected { get; set; }
}
