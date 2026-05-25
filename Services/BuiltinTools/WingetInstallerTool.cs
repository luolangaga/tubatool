using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using TubaWinUi3.Models;
using TubaWinUi3.Services;
using Windows.UI;

namespace TubaWinUi3.Services;

public sealed class WingetInstallerTool : IBuiltinTool
{
    public string Id => "winget-installer";
    public string Name => "软件安装";
    public string Description => "通过 winget 一键安装常用软件，支持批量选择与进度显示。";
    public string Glyph => "\uE896";
    public string Category => "系统工具";
    public BuiltinToolKind Kind => BuiltinToolKind.ProgressTask;

    private static readonly Color DimText = Color.FromArgb(255, 140, 140, 140);
    private static readonly Color BorderColor = Color.FromArgb(255, 60, 60, 60);
    private static readonly Color CardBg = Color.FromArgb(255, 45, 45, 45);
    private static readonly Color AccentGreen = Color.FromArgb(255, 74, 222, 128);
    private static readonly Color AccentBlue = Color.FromArgb(255, 96, 165, 250);
    private static readonly Color AccentOrange = Color.FromArgb(255, 251, 191, 36);
    private static readonly Color AccentRed = Color.FromArgb(255, 248, 113, 113);

    private List<WingetPackage>? _packages;
    private CancellationTokenSource? _cts;
    private bool _isInstalling;

    public async Task ExecuteAsync(BuiltinToolContext context)
    {
        var available = await WingetService.IsWingetAvailableAsync();
        if (!available)
        {
            var errDialog = new ContentDialog
            {
                Title = "winget 不可用",
                Content = "未检测到 winget，请确认系统已安装 App Installer 并更新至最新版本。",
                CloseButtonText = "确定",
                XamlRoot = context.XamlRoot
            };
            await errDialog.ShowAsync();
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "软件安装",
            CloseButtonText = "关闭",
            XamlRoot = context.XamlRoot
        };
        dialog.Resources["ContentDialogMaxWidth"] = 960;
        dialog.Resources["ContentDialogMaxHeight"] = 720;
        dialog.Closing += (_, args) =>
        {
            if (_isInstalling)
            {
                args.Cancel = true;
            }
            else
            {
                _cts?.Cancel();
            }
        };

        var content = BuildDialogContent();
        dialog.Content = content;

        _ = dialog.ShowAsync();
        await CheckInstalledStatus(content);
    }

    private ScrollViewer BuildDialogContent()
    {
        var totalText = new TextBlock { FontSize = 22, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = new SolidColorBrush(AccentBlue) };
        var installedText = new TextBlock { FontSize = 22, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = new SolidColorBrush(AccentGreen) };
        var selectedText = new TextBlock { FontSize = 22, FontWeight = Microsoft.UI.Text.FontWeights.Bold, Foreground = new SolidColorBrush(AccentOrange) };

        var totalCard = MakeStatCard("可用软件", totalText, "\uE896", AccentBlue);
        var installedCard = MakeStatCard("已安装", installedText, "\uE73E", AccentGreen);
        var selectedCard = MakeStatCard("待安装", selectedText, "\uE916", AccentOrange);

        var statsGrid = new Grid { ColumnSpacing = 10 };
        statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        statsGrid.Children.Add(totalCard); Grid.SetColumn(totalCard, 0);
        statsGrid.Children.Add(installedCard); Grid.SetColumn(installedCard, 1);
        statsGrid.Children.Add(selectedCard); Grid.SetColumn(selectedCard, 2);

        var categoryFilter = new ComboBox { MinWidth = 140, PlaceholderText = "全部分类" };
        categoryFilter.Items.Add("全部分类");
        foreach (var cat in WingetService.GetCategories())
        {
            categoryFilter.Items.Add(cat);
        }
        categoryFilter.SelectedIndex = 0;

        var installBtn = new Button
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new FontIcon { Glyph = "\uE896", FontSize = 12 },
                    new TextBlock { Text = "安装选中" }
                }
            },
            Style = Application.Current.Resources["AccentButtonStyle"] as Style
        };

        var selectAllBtn = new Button { Content = "全选", Padding = new Thickness(8, 4, 8, 4) };
        var deselectAllBtn = new Button { Content = "取消全选", Padding = new Thickness(8, 4, 8, 4) };
        var selectNotInstalledBtn = new Button { Content = "选中未安装", Padding = new Thickness(8, 4, 8, 4) };

        var actionBar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        actionBar.Children.Add(categoryFilter);
        actionBar.Children.Add(installBtn);
        actionBar.Children.Add(selectAllBtn);
        actionBar.Children.Add(deselectAllBtn);
        actionBar.Children.Add(selectNotInstalledBtn);

        var packageList = new StackPanel { Spacing = 6 };
        var listScroll = new ScrollViewer
        {
            Content = packageList,
            MaxHeight = 420,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var loadingRing = new ProgressRing { Width = 36, Height = 36, IsActive = false };
        var loadingText = new TextBlock { Text = "", FontSize = 12, Foreground = new SolidColorBrush(DimText) };
        var loadingPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 8,
            Padding = new Thickness(0, 6, 0, 6),
            Children = { loadingRing, loadingText }
        };
        loadingPanel.Visibility = Visibility.Collapsed;

        var resultText = new TextBlock
        {
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(AccentGreen),
            Visibility = Visibility.Collapsed
        };

        var root = new StackPanel { Spacing = 14, MaxWidth = 900 };
        root.Children.Add(new TextBlock
        {
            Text = "通过 Windows 包管理器 (winget) 快速安装常用软件，勾选需要的软件后点击安装",
            FontSize = 12,
            Foreground = new SolidColorBrush(DimText)
        });
        root.Children.Add(statsGrid);
        root.Children.Add(actionBar);
        root.Children.Add(loadingPanel);
        root.Children.Add(listScroll);
        root.Children.Add(resultText);

        var scrollViewer = new ScrollViewer { Content = root, MaxWidth = 960 };
        scrollViewer.Tag = new WingetInstallerState
        {
            TotalText = totalText,
            InstalledText = installedText,
            SelectedText = selectedText,
            InstallBtn = installBtn,
            SelectAllBtn = selectAllBtn,
            DeselectAllBtn = deselectAllBtn,
            SelectNotInstalledBtn = selectNotInstalledBtn,
            CategoryFilter = categoryFilter,
            PackageList = packageList,
            ListScroll = listScroll,
            LoadingRing = loadingRing,
            LoadingText = loadingText,
            LoadingPanel = loadingPanel,
            ResultText = resultText
        };

        categoryFilter.SelectionChanged += (_, _) =>
        {
            var selected = categoryFilter.SelectedItem as string;
            RenderPackages(scrollViewer, selected == "全部分类" ? null : selected);
        };

        installBtn.Click += async (_, _) =>
        {
            await InstallSelectedAsync(scrollViewer);
        };

        selectAllBtn.Click += (_, _) =>
        {
            if (_packages is null) return;
            foreach (var p in _packages) p.IsSelected = true;
            RenderPackages(scrollViewer, GetCurrentFilter(scrollViewer));
        };

        deselectAllBtn.Click += (_, _) =>
        {
            if (_packages is null) return;
            foreach (var p in _packages) p.IsSelected = false;
            RenderPackages(scrollViewer, GetCurrentFilter(scrollViewer));
        };

        selectNotInstalledBtn.Click += (_, _) =>
        {
            if (_packages is null) return;
            foreach (var p in _packages) p.IsSelected = p.State != WingetInstallState.Installed;
            RenderPackages(scrollViewer, GetCurrentFilter(scrollViewer));
        };

        LoadCatalog(scrollViewer);

        return scrollViewer;
    }

    private void LoadCatalog(ScrollViewer root)
    {
        _packages = WingetService.GetCatalog();
        RenderPackages(root, null);
    }

    private async Task CheckInstalledStatus(ScrollViewer root)
    {
        var state = GetState(root);
        if (state is null || _packages is null) return;

        state.LoadingPanel.Visibility = Visibility.Visible;
        state.LoadingRing.IsActive = true;
        state.LoadingText.Text = "正在检测已安装软件...";
        state.InstallBtn.IsEnabled = false;

        var packages = _packages.ToList();

        await Task.Run(async () =>
        {
            var tasks = packages.Select(async p =>
            {
                var installed = await WingetService.IsInstalledAsync(p.Id);
                p.State = installed ? WingetInstallState.Installed : WingetInstallState.NotInstalled;
                p.StatusText = installed ? "已安装" : "未安装";
            });

            await Task.WhenAll(tasks);
        });

        RefreshStats(root);
        RenderPackages(root, GetCurrentFilter(root));

        state.LoadingPanel.Visibility = Visibility.Collapsed;
        state.LoadingRing.IsActive = false;
        state.InstallBtn.IsEnabled = true;
    }

    private async Task InstallSelectedAsync(ScrollViewer root)
    {
        var state = GetState(root);
        if (state is null || _packages is null) return;

        var toInstall = _packages.Where(p => p.IsSelected && p.State != WingetInstallState.Installed).ToList();
        if (toInstall.Count == 0)
        {
            state.ResultText.Text = "没有需要安装的软件";
            state.ResultText.Foreground = new SolidColorBrush(AccentOrange);
            state.ResultText.Visibility = Visibility.Visible;
            return;
        }

        _isInstalling = true;
        state.InstallBtn.IsEnabled = false;
        state.SelectAllBtn.IsEnabled = false;
        state.DeselectAllBtn.IsEnabled = false;
        state.SelectNotInstalledBtn.IsEnabled = false;
        state.CategoryFilter.IsEnabled = false;
        state.ResultText.Visibility = Visibility.Collapsed;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        var succeeded = 0;
        var failed = 0;

        foreach (var pkg in toInstall)
        {
            _cts.Token.ThrowIfCancellationRequested();

            pkg.State = WingetInstallState.Installing;
            pkg.StatusText = "正在安装...";
            pkg.Progress = 0;
            RefreshPackageRow(root, pkg);

            var progress = new Progress<WingetInstallProgress>(p =>
            {
                pkg.StatusText = p.StatusLine;
                pkg.Progress = p.Percent;
                RefreshPackageRow(root, pkg);
            });

            try
            {
                var result = await WingetService.InstallAsync(pkg.Id, progress, _cts.Token);
                pkg.State = result.Success ? WingetInstallState.Succeeded : WingetInstallState.Failed;
                pkg.StatusText = result.Message;
                pkg.Progress = result.Success ? 100 : 0;

                if (result.Success) succeeded++;
                else failed++;
            }
            catch (OperationCanceledException)
            {
                pkg.State = WingetInstallState.Failed;
                pkg.StatusText = "已取消";
                pkg.Progress = 0;
                failed++;
                break;
            }

            RefreshPackageRow(root, pkg);
        }

        RefreshStats(root);

        state.ResultText.Text = failed == 0
            ? $"全部安装完成！成功安装 {succeeded} 个软件"
            : $"安装完成：成功 {succeeded} 个，失败 {failed} 个";
        state.ResultText.Foreground = new SolidColorBrush(failed == 0 ? AccentGreen : AccentOrange);
        state.ResultText.Visibility = Visibility.Visible;

        state.InstallBtn.IsEnabled = true;
        state.SelectAllBtn.IsEnabled = true;
        state.DeselectAllBtn.IsEnabled = true;
        state.SelectNotInstalledBtn.IsEnabled = true;
        state.CategoryFilter.IsEnabled = true;
        _isInstalling = false;
    }

    private void RenderPackages(ScrollViewer root, string? category)
    {
        var state = GetState(root);
        if (state is null || _packages is null) return;

        state.PackageList.Children.Clear();

        var filtered = category is null
            ? _packages
            : _packages.Where(p => p.Category == category).ToList();

        var currentCategory = "";
        foreach (var pkg in filtered)
        {
            if (pkg.Category != currentCategory)
            {
                currentCategory = pkg.Category;
                var header = new TextBlock
                {
                    Text = currentCategory,
                    FontSize = 14,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 180, 180, 180)),
                    Margin = new Thickness(0, 8, 0, 2)
                };
                state.PackageList.Children.Add(header);
            }

            state.PackageList.Children.Add(CreatePackageRow(pkg, root));
        }

        RefreshStats(root);
    }

    private Border CreatePackageRow(WingetPackage pkg, ScrollViewer root)
    {
        var stateColor = pkg.State switch
        {
            WingetInstallState.Installed => AccentGreen,
            WingetInstallState.Succeeded => AccentGreen,
            WingetInstallState.Failed => AccentRed,
            WingetInstallState.Installing => AccentBlue,
            _ => DimText
        };

        var iconBorder = new Border
        {
            Width = 36,
            Height = 36,
            Background = new SolidColorBrush(Color.FromArgb(26, stateColor.R, stateColor.G, stateColor.B)),
            CornerRadius = new CornerRadius(6),
            Child = new FontIcon { FontSize = 16, Foreground = new SolidColorBrush(stateColor), Glyph = pkg.Glyph }
        };

        var nameText = new TextBlock
        {
            Text = pkg.Name,
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 210, 210, 210))
        };

        var descText = new TextBlock
        {
            Text = pkg.Description ?? "",
            FontSize = 11,
            Foreground = new SolidColorBrush(DimText)
        };

        var infoPanel = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        infoPanel.Children.Add(nameText);
        infoPanel.Children.Add(descText);

        var statusText = new TextBlock
        {
            Text = pkg.StatusText ?? GetDefaultStatusText(pkg.State),
            FontSize = 12,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = new SolidColorBrush(stateColor),
            VerticalAlignment = VerticalAlignment.Center
        };

        var progressBar = new ProgressBar
        {
            Value = pkg.Progress,
            Minimum = 0,
            Maximum = 100,
            Width = 80,
            VerticalAlignment = VerticalAlignment.Center,
            Visibility = pkg.State == WingetInstallState.Installing ? Visibility.Visible : Visibility.Collapsed
        };

        var statusPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        statusPanel.Children.Add(progressBar);
        statusPanel.Children.Add(statusText);

        var checkbox = new CheckBox
        {
            IsChecked = pkg.IsSelected,
            MinWidth = 28,
            VerticalAlignment = VerticalAlignment.Center
        };
        checkbox.Checked += (_, _) =>
        {
            pkg.IsSelected = true;
            RefreshStats(root);
        };
        checkbox.Unchecked += (_, _) =>
        {
            pkg.IsSelected = false;
            RefreshStats(root);
        };

        if (pkg.State == WingetInstallState.Installed || pkg.State == WingetInstallState.Installing)
        {
            checkbox.IsEnabled = pkg.State != WingetInstallState.Installing;
        }

        var grid = new Grid { ColumnSpacing = 12 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.Children.Add(checkbox);
        grid.Children.Add(iconBorder); Grid.SetColumn(iconBorder, 1);
        grid.Children.Add(infoPanel); Grid.SetColumn(infoPanel, 2);
        grid.Children.Add(statusPanel); Grid.SetColumn(statusPanel, 3);

        var rowBorder = new Border
        {
            Padding = new Thickness(12, 8, 12, 8),
            Background = new SolidColorBrush(CardBg),
            BorderBrush = new SolidColorBrush(BorderColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = grid,
            Tag = pkg.Id
        };

        return rowBorder;
    }

    private void RefreshPackageRow(ScrollViewer root, WingetPackage pkg)
    {
        var state = GetState(root);
        if (state is null) return;

        var row = state.PackageList.Children.OfType<Border>().FirstOrDefault(b => (string?)b.Tag == pkg.Id);
        if (row is null) return;

        var index = state.PackageList.Children.IndexOf(row);
        var category = GetCurrentFilter(root);

        var newRow = CreatePackageRow(pkg, root);
        state.PackageList.Children.RemoveAt(index);
        state.PackageList.Children.Insert(index, newRow);

        RefreshStats(root);
    }

    private void RefreshStats(ScrollViewer root)
    {
        var state = GetState(root);
        if (state is null || _packages is null) return;

        var category = GetCurrentFilter(root);
        var filtered = category is null
            ? _packages
            : _packages.Where(p => p.Category == category).ToList();

        var total = filtered.Count;
        var installed = filtered.Count(p => p.State is WingetInstallState.Installed or WingetInstallState.Succeeded);
        var selected = filtered.Count(p => p.IsSelected && p.State != WingetInstallState.Installed);

        state.TotalText.Text = total.ToString();
        state.InstalledText.Text = installed.ToString();
        state.SelectedText.Text = selected.ToString();
    }

    private static string? GetCurrentFilter(ScrollViewer root)
    {
        var state = GetState(root);
        if (state is null) return null;
        var selected = state.CategoryFilter.SelectedItem as string;
        return selected == "全部分类" ? null : selected;
    }

    private static string GetDefaultStatusText(WingetInstallState state) => state switch
    {
        WingetInstallState.NotInstalled => "未安装",
        WingetInstallState.Checking => "检测中...",
        WingetInstallState.Installed => "已安装",
        WingetInstallState.Installing => "安装中...",
        WingetInstallState.Succeeded => "安装成功",
        WingetInstallState.Failed => "安装失败",
        WingetInstallState.Skipped => "已跳过",
        _ => ""
    };

    private static WingetInstallerState? GetState(ScrollViewer root) => root?.Tag as WingetInstallerState;

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

    private sealed class WingetInstallerState
    {
        public TextBlock TotalText = null!;
        public TextBlock InstalledText = null!;
        public TextBlock SelectedText = null!;
        public Button InstallBtn = null!;
        public Button SelectAllBtn = null!;
        public Button DeselectAllBtn = null!;
        public Button SelectNotInstalledBtn = null!;
        public ComboBox CategoryFilter = null!;
        public StackPanel PackageList = null!;
        public ScrollViewer ListScroll = null!;
        public ProgressRing LoadingRing = null!;
        public TextBlock LoadingText = null!;
        public StackPanel LoadingPanel = null!;
        public TextBlock ResultText = null!;
    }
}
