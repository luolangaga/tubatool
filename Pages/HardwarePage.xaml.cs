using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TubaWinUi3.Models;
using TubaWinUi3.Services;

namespace TubaWinUi3.Pages;

public sealed partial class HardwarePage : Page
{
    private DispatcherTimer? _uptimeTimer;

    public HardwarePage()
    {
        InitializeComponent();
        Loaded += HardwarePage_Loaded;
        Unloaded += HardwarePage_Unloaded;
    }

    private void HardwarePage_Loaded(object sender, RoutedEventArgs e)
    {
        _ = LoadHardwareInfoAsync();

        _uptimeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _uptimeTimer.Tick += (_, _) => UpdateUptime();
        _uptimeTimer.Start();
    }

    private void HardwarePage_Unloaded(object sender, RoutedEventArgs e)
    {
        _uptimeTimer?.Stop();
        _uptimeTimer = null;
    }

    private void UpdateUptime()
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        UptimeText.Text = $"{uptime.Days}天{uptime.Hours}小时{uptime.Minutes}分钟{uptime.Seconds}秒";
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadHardwareInfoAsync();
    }

    private async Task LoadHardwareInfoAsync()
    {
        SetLoading(true);

        try
        {
            var sections = await HardwareInfoService.LoadAsync();
            ApplySections(sections);
            StatusBar.IsOpen = false;
        }
        catch (Exception ex)
        {
            ModelText.Text = "未知";
            SystemText.Text = "未知";
            UptimeText.Text = "未知";
            DetailsRepeater.ItemsSource = Array.Empty<HardwareInfoItem>();
            StatusBar.Title = "硬件信息读取失败";
            StatusBar.Message = ex.Message;
            StatusBar.Severity = InfoBarSeverity.Error;
            StatusBar.IsOpen = true;
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void ApplySections(IReadOnlyList<HardwareInfoSection> sections)
    {
        var summary = sections[0].Items;
        var system = sections[1].Items;
        var details = sections[2].Items;

        ModelText.Text = summary.FirstOrDefault(item => item.Label == "设备型号")?.Value ?? "未知";
        SystemText.Text = system.FirstOrDefault(item => item.Label == "系统")?.Value ?? "未知";
        UpdateUptime();
        DetailsRepeater.ItemsSource = details;
    }

    private void SetLoading(bool isLoading)
    {
        LoadingRing.IsActive = isLoading;
        LoadingRing.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
    }
}
