using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using TubaWinUi3.Services;

namespace TubaWinUi3.Services;

public sealed class BatteryReportTool : IBuiltinTool
{
    public string Id => "battery-report";
    public string Name => "电池报告";
    public string Description => "查看笔记本电池健康度、设计容量、充满容量和充放电周期。";
    public string Glyph => "\uE85A";
    public string Category => "硬件信息";
    public BuiltinToolKind Kind => BuiltinToolKind.ProgressTask;

    private static readonly Color AccentGreen = Color.FromArgb(255, 74, 222, 128);
    private static readonly Color AccentBlue = Color.FromArgb(255, 96, 165, 250);
    private static readonly Color AccentOrange = Color.FromArgb(255, 251, 191, 36);
    private static readonly Color AccentRed = Color.FromArgb(255, 248, 113, 113);
    private static readonly Color DimText = Color.FromArgb(255, 140, 140, 140);
    private static readonly Color BorderColor = Color.FromArgb(255, 60, 60, 60);
    private static readonly Color CardBg = Color.FromArgb(255, 45, 45, 45);

    public async Task ExecuteAsync(BuiltinToolContext context)
    {
        var dialog = new ContentDialog
        {
            Title = "电池报告",
            CloseButtonText = "关闭",
            XamlRoot = context.XamlRoot
        };
        dialog.Resources["ContentDialogMaxWidth"] = 860;

        var content = BuildDialogContent();
        dialog.Content = content;

        _ = LoadBatteryInfoAsync(content);
        await dialog.ShowAsync();
    }

    private async Task LoadBatteryInfoAsync(ScrollViewer root)
    {
        var state = GetState(root);
        if (state is null) return;

        state.LoadingPanel.Visibility = Visibility.Visible;
        state.LoadingRing.IsActive = true;

        var info = await BatteryReportService.GetBatteryInfoAsync();

        if (!info.BatteryPresent)
        {
            state.LoadingPanel.Visibility = Visibility.Collapsed;
            state.NoBatteryPanel.Visibility = Visibility.Visible;
            state.InfoPanel.Visibility = Visibility.Collapsed;
            return;
        }

        state.LoadingPanel.Visibility = Visibility.Collapsed;
        state.NoBatteryPanel.Visibility = Visibility.Collapsed;
        state.InfoPanel.Visibility = Visibility.Visible;

        var healthColor = info.HealthPercent >= 80 ? AccentGreen
            : info.HealthPercent >= 60 ? AccentOrange
            : info.HealthPercent >= 40 ? AccentRed
            : AccentRed;

        state.HealthPercentText.Text = $"{info.HealthPercent}%";
        state.HealthPercentText.Foreground = new SolidColorBrush(healthColor);
        state.HealthBar.Value = info.HealthPercent;
        state.HealthBar.Foreground = new SolidColorBrush(healthColor);

        state.HealthStatusText.Text = info.HealthStatus;
        state.HealthStatusText.Foreground = new SolidColorBrush(healthColor);

        state.ChargePercentText.Text = $"{info.EstimatedChargeRemaining}%";
        state.BatteryStatusText.Text = info.BatteryStatus;

        state.DesignCapacityText.Text = info.DesignedCapacityText;
        state.FullChargeCapacityText.Text = info.FullChargedCapacityText;
        state.CycleCountText.Text = info.CycleCount > 0 ? info.CycleCount.ToString() : "未知";
        state.ManufacturerText.Text = info.ManufactureName;
        state.ManufactureDateText.Text = info.ManufactureDate;

        state.ExportBtn.Click += async (_, _) =>
        {
            var path = await BatteryReportService.ExportHtmlReportAsync();
            if (!string.IsNullOrEmpty(path))
            {
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true }); }
                catch { }
            }
        };
        state.NoBatteryExportBtn.Click += async (_, _) =>
        {
            var path = await BatteryReportService.ExportHtmlReportAsync();
            if (!string.IsNullOrEmpty(path))
            {
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true }); }
                catch { }
            }
        };
    }

    private ScrollViewer BuildDialogContent()
    {
        var healthPercentText = new TextBlock { FontSize = 28, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
        var healthBar = new ProgressBar { Minimum = 0, Maximum = 100, Width = 300, HorizontalAlignment = HorizontalAlignment.Left };
        var healthStatusText = new TextBlock { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold };

        var chargePercentText = new TextBlock { FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = new SolidColorBrush(AccentBlue) };
        var batteryStatusText = new TextBlock { FontSize = 14, Foreground = new SolidColorBrush(DimText) };

        var designCapacityText = new TextBlock { FontSize = 14, Foreground = new SolidColorBrush(Color.FromArgb(255, 210, 210, 210)) };
        var fullChargeCapacityText = new TextBlock { FontSize = 14, Foreground = new SolidColorBrush(Color.FromArgb(255, 210, 210, 210)) };
        var cycleCountText = new TextBlock { FontSize = 14, Foreground = new SolidColorBrush(Color.FromArgb(255, 210, 210, 210)) };
        var manufacturerText = new TextBlock { FontSize = 14, Foreground = new SolidColorBrush(Color.FromArgb(255, 210, 210, 210)) };
        var manufactureDateText = new TextBlock { FontSize = 14, Foreground = new SolidColorBrush(Color.FromArgb(255, 210, 210, 210)) };

        var exportBtn = new Button
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new FontIcon { Glyph = "\uE8A5", FontSize = 12 },
                    new TextBlock { Text = "导出完整报告" }
                }
            }
        };

        var healthCard = MakeStatCard("电池健康度", healthPercentText, "\uE85A", AccentGreen);
        var chargeCard = MakeStatCard("当前电量", chargePercentText, "\uE85A", AccentBlue);

        var statsGrid = new Grid { ColumnSpacing = 10 };
        statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        statsGrid.Children.Add(healthCard); Grid.SetColumn(healthCard, 0);
        statsGrid.Children.Add(chargeCard); Grid.SetColumn(chargeCard, 1);

        var healthPanel = new StackPanel { Spacing = 8 };
        healthPanel.Children.Add(statsGrid);
        healthPanel.Children.Add(new StackPanel { Spacing = 4 });
        healthPanel.Children.Add(healthBar);
        healthPanel.Children.Add(new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 });
        healthPanel.Children.Add(healthStatusText);
        healthPanel.Children.Add(batteryStatusText);

        var detailsPanel = new StackPanel { Spacing = 12 };
        detailsPanel.Children.Add(MakeDetailRow("设计容量", designCapacityText, "\uEDA2"));
        detailsPanel.Children.Add(MakeDetailRow("充满容量", fullChargeCapacityText, "\uEDA2"));
        detailsPanel.Children.Add(MakeDetailRow("循环次数", cycleCountText, "\uE8C8"));
        detailsPanel.Children.Add(MakeDetailRow("制造商", manufacturerText, "\uE7F4"));
        detailsPanel.Children.Add(MakeDetailRow("制造日期", manufactureDateText, "\uE787"));
        detailsPanel.Children.Add(exportBtn);

        var infoPanel = new StackPanel { Spacing = 14 };
        infoPanel.Children.Add(healthPanel);
        infoPanel.Children.Add(MakeSectionHeader("详细信息"));
        infoPanel.Children.Add(detailsPanel);
        infoPanel.Visibility = Visibility.Collapsed;

        var loadingRing = new ProgressRing { Width = 40, Height = 40, IsActive = true };
        var loadingText = new TextBlock { Text = "正在获取电池信息...", FontSize = 13, Foreground = new SolidColorBrush(DimText) };
        var loadingPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 8,
            Padding = new Thickness(0, 30, 0, 30),
            Children = { loadingRing, loadingText }
        };

        var noBatteryExportBtn = new Button
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new FontIcon { Glyph = "\uE8A5", FontSize = 12 },
                    new TextBlock { Text = "导出完整报告" }
                }
            }
        };

        var noBatteryPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 12,
            Padding = new Thickness(0, 40, 0, 40),
            Children =
            {
                new FontIcon { Glyph = "\uE85A", FontSize = 48, Foreground = new SolidColorBrush(DimText) },
                new TextBlock { Text = "未检测到电池", FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromArgb(255, 210, 210, 210)) },
                new TextBlock { Text = "此设备可能为台式机或未安装电池", FontSize = 12, Foreground = new SolidColorBrush(DimText) },
                noBatteryExportBtn
            }
        };
        noBatteryPanel.Visibility = Visibility.Collapsed;

        var rootStack = new StackPanel { Spacing = 14, MaxWidth = 800 };
        rootStack.Children.Add(new TextBlock
        {
            Text = "查看笔记本电池健康度、设计容量与充满容量对比、充放电周期计数",
            FontSize = 12,
            Foreground = new SolidColorBrush(DimText)
        });
        rootStack.Children.Add(infoPanel);
        rootStack.Children.Add(loadingPanel);
        rootStack.Children.Add(noBatteryPanel);

        var scrollViewer = new ScrollViewer { Content = rootStack, MaxWidth = 860 };
        scrollViewer.Tag = new BatteryReportState
        {
            HealthPercentText = healthPercentText,
            HealthBar = healthBar,
            HealthStatusText = healthStatusText,
            ChargePercentText = chargePercentText,
            BatteryStatusText = batteryStatusText,
            DesignCapacityText = designCapacityText,
            FullChargeCapacityText = fullChargeCapacityText,
            CycleCountText = cycleCountText,
            ManufacturerText = manufacturerText,
            ManufactureDateText = manufactureDateText,
            ExportBtn = exportBtn,
            NoBatteryExportBtn = noBatteryExportBtn,
            LoadingPanel = loadingPanel,
            LoadingRing = loadingRing,
            InfoPanel = infoPanel,
            NoBatteryPanel = noBatteryPanel
        };

        return scrollViewer;
    }

    private static Border MakeDetailRow(string label, TextBlock value, string glyph)
    {
        var labelBlock = new TextBlock { Text = label, FontSize = 11, Foreground = new SolidColorBrush(DimText), VerticalAlignment = VerticalAlignment.Center };
        var icon = new FontIcon { FontSize = 14, Glyph = glyph, Foreground = new SolidColorBrush(DimText), VerticalAlignment = VerticalAlignment.Center };

        var grid = new Grid { ColumnSpacing = 10 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.Children.Add(icon);
        grid.Children.Add(labelBlock); Grid.SetColumn(labelBlock, 1);
        grid.Children.Add(value); Grid.SetColumn(value, 2);

        return new Border
        {
            Padding = new Thickness(10, 8, 10, 8),
            Background = new SolidColorBrush(CardBg),
            BorderBrush = new SolidColorBrush(BorderColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = grid
        };
    }

    private static TextBlock MakeSectionHeader(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 180, 180, 180))
        };
    }

    private static Border MakeStatCard(string label, TextBlock value, string glyph, Color accent)
    {
        var iconBorder = new Border
        {
            Width = 36,
            Height = 36,
            Background = new SolidColorBrush(Color.FromArgb(26, accent.R, accent.G, accent.B)),
            CornerRadius = new CornerRadius(6),
            Child = new FontIcon { FontSize = 16, Foreground = new SolidColorBrush(accent), Glyph = glyph }
        };
        var labelBlock = new TextBlock { Text = label, FontSize = 11, Foreground = new SolidColorBrush(DimText) };
        var stack = new StackPanel { Spacing = 2 };
        stack.Children.Add(labelBlock);
        stack.Children.Add(value);

        var grid = new Grid { ColumnSpacing = 10 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.Children.Add(iconBorder);
        grid.Children.Add(stack); Grid.SetColumn(stack, 1);

        return new Border
        {
            Padding = new Thickness(12),
            Background = new SolidColorBrush(CardBg),
            BorderBrush = new SolidColorBrush(BorderColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = grid
        };
    }

    private static BatteryReportState? GetState(ScrollViewer root) => root?.Tag as BatteryReportState;

    private sealed class BatteryReportState
    {
        public TextBlock HealthPercentText = null!;
        public ProgressBar HealthBar = null!;
        public TextBlock HealthStatusText = null!;
        public TextBlock ChargePercentText = null!;
        public TextBlock BatteryStatusText = null!;
        public TextBlock DesignCapacityText = null!;
        public TextBlock FullChargeCapacityText = null!;
        public TextBlock CycleCountText = null!;
        public TextBlock ManufacturerText = null!;
        public TextBlock ManufactureDateText = null!;
        public Button ExportBtn = null!;
        public Button NoBatteryExportBtn = null!;
        public StackPanel LoadingPanel = null!;
        public ProgressRing LoadingRing = null!;
        public StackPanel InfoPanel = null!;
        public StackPanel NoBatteryPanel = null!;
    }
}