using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using TubaWinUi3.Models;
using TubaWinUi3.Services;
using Windows.ApplicationModel.DataTransfer;

namespace TubaWinUi3.Pages;

public sealed partial class HardwarePage : Page
{
    private DispatcherTimer? _uptimeTimer;
    private bool _dataLoaded;
    private bool _animatingDetails;

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

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        _ = LoadHardwareInfoAsync(forceRefresh: true);
    }

    private void Card_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Border border) return;
        var sb = new Storyboard();
        var scaleX = new DoubleAnimation { To = 1.02, Duration = TimeSpan.FromMilliseconds(120), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
        var scaleY = new DoubleAnimation { To = 1.02, Duration = TimeSpan.FromMilliseconds(120), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
        Storyboard.SetTarget(scaleX, border);
        Storyboard.SetTarget(scaleY, border);
        Storyboard.SetTargetProperty(scaleX, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
        Storyboard.SetTargetProperty(scaleY, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");
        sb.Children.Add(scaleX);
        sb.Children.Add(scaleY);
        sb.Begin();
    }

    private void Card_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Border border) return;
        var sb = new Storyboard();
        var scaleX = new DoubleAnimation { To = 1.0, Duration = TimeSpan.FromMilliseconds(180), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
        var scaleY = new DoubleAnimation { To = 1.0, Duration = TimeSpan.FromMilliseconds(180), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
        Storyboard.SetTarget(scaleX, border);
        Storyboard.SetTarget(scaleY, border);
        Storyboard.SetTargetProperty(scaleX, "(UIElement.RenderTransform).(ScaleTransform.ScaleX)");
        Storyboard.SetTargetProperty(scaleY, "(UIElement.RenderTransform).(ScaleTransform.ScaleY)");
        sb.Children.Add(scaleX);
        sb.Children.Add(scaleY);
        sb.Begin();
    }

    private async Task LoadHardwareInfoAsync(bool forceRefresh = false)
    {
        if (_dataLoaded)
        {
            ExitStoryboard.Begin();
            await Task.Delay(200);
        }

        SetLoading(true);

        try
        {
            var sections = await HardwareInfoService.LoadAsync(forceRefresh);
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
        _animatingDetails = true;
        DetailsRepeater.ItemsSource = details;

        EntranceStoryboard.Begin();
        _dataLoaded = true;
    }

    private void SetLoading(bool isLoading)
    {
        LoadingRing.IsActive = isLoading;
        LoadingRing.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Card1_Tapped(object sender, TappedRoutedEventArgs e) => CopyToClipboard(ModelText.Text);
    private void Card2_Tapped(object sender, TappedRoutedEventArgs e) => CopyToClipboard(SystemText.Text);
    private void Card3_Tapped(object sender, TappedRoutedEventArgs e) => CopyToClipboard(UptimeText.Text);

    private void DetailItem_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
    }

    private void DetailItem_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not Border border) return;
        if (border.DataContext is not HardwareInfoItem item) return;
        CopyToClipboard(item.Value);
    }

    private void CopyToClipboard(string text)
    {
        var dp = new DataPackage();
        dp.SetText(text);
        Clipboard.SetContent(dp);
        ShowCopyToast(text);
    }

    private void ShowCopyToast(string text)
    {
        StatusBar.Title = "已复制";
        StatusBar.Message = text.Length > 80 ? text[..80] + "…" : text;
        StatusBar.Severity = InfoBarSeverity.Success;
        StatusBar.IsOpen = true;
    }

    private void DetailsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (!_animatingDetails || args.Index < 0 || args.Element is not Border el) return;

        var idx = (int)args.Index;
        el.Opacity = 0;

        var delay = TimeSpan.FromMilliseconds(350 + idx * 60);
        var lastIdx = ((IReadOnlyList<HardwareInfoItem>)DetailsRepeater.ItemsSource!).Count - 1;

        var timer = new DispatcherTimer { Interval = delay };
        timer.Tick += (_, _) =>
        {
            timer.Stop();

            var sb = new Storyboard();
            var fade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(fade, el);
            Storyboard.SetTargetProperty(fade, "Opacity");
            sb.Children.Add(fade);

            sb.Begin();

            if (idx == lastIdx) _animatingDetails = false;
        };
        timer.Start();
    }
}
