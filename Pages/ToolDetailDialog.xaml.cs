using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using TubaWinUi3.Models;
using TubaWinUi3.Services;

namespace TubaWinUi3.Pages;

public sealed partial class ToolDetailDialog : UserControl
{
    private ToolItem? _tool;
    private Visual? _smokeVisual;
    private Visual? _contentVisual;
    private Visual? _heroVisual;
    private bool _isClosing;
    private bool _isOpen;
    private Popup? _popup;

    public event Action<ToolItem>? ToolLaunched;
    public event Action<ToolItem>? FavoriteChanged;

    public ToolDetailDialog()
    {
        InitializeComponent();
    }

    public async Task ShowAsync(ToolItem tool, FrameworkElement? sourceCard)
    {
        _tool = tool;
        _isClosing = false;
        _isOpen = false;
        PopulateUI(tool);

        var xamlRoot = App.MainWindow?.Content?.XamlRoot;
        if (xamlRoot is null) return;

        _popup = new Popup
        {
            XamlRoot = xamlRoot,
            IsLightDismissEnabled = true,
            LightDismissOverlayMode = LightDismissOverlayMode.Off
        };

        this.Width = xamlRoot.Size.Width;
        this.Height = xamlRoot.Size.Height;
        _popup.Child = this;
        _popup.IsOpen = true;
        _isOpen = true;

        _popup.Closed += OnPopupClosed;

        PrepareAnimation();

        await Task.Delay(16);
        PlayOpenAnimation();
    }

    private void OnPopupClosed(object? sender, object e)
    {
        _isOpen = false;
    }

    private void Close()
    {
        if (_isClosing || !_isOpen) return;
        PlayCloseAnimation();
    }

    private void PopulateUI(ToolItem tool)
    {
        HeroTitle.Text = tool.Name;
        HeroCategory.Text = tool.Category;

        if (!string.IsNullOrEmpty(tool.Extension))
        {
            HeroExtBadge.Visibility = Visibility.Visible;
            HeroExtText.Text = tool.Extension.ToUpperInvariant();
        }

        if (!string.IsNullOrEmpty(tool.IconPath))
        {
            HeroIconImage.Source = new BitmapImage(new Uri(tool.IconPath));
            HeroIconImage.Visibility = Visibility.Visible;
        }
        else if (!string.IsNullOrEmpty(tool.IconGlyph))
        {
            HeroIconGlyph.Glyph = tool.IconGlyph;
            HeroIconGlyph.Visibility = Visibility.Visible;
        }
        else
        {
            HeroIconGlyph.Glyph = "\uE8B7";
            HeroIconGlyph.Visibility = Visibility.Visible;
        }

        DescriptionText.Text = string.IsNullOrWhiteSpace(tool.Description)
            ? "暂无介绍。"
            : tool.Description;

        PublisherText.Text = ValueOrUnknown(tool.Publisher);
        VersionText.Text = ValueOrUnknown(tool.Version);
        FileTypeText.Text = tool.Extension.ToUpperInvariant();
        PathText.Text = tool.Path;

        AdminButton.Visibility = tool.NeedsDownload ? Visibility.Collapsed : Visibility.Visible;
        LaunchButtonText.Text = tool.LaunchButtonText;

        UpdateFavoriteIcon(tool.IsFavorite);

        LoadReadme(tool);
    }

    private void LoadReadme(ToolItem tool)
    {
        try
        {
            var dir = Path.GetDirectoryName(tool.Path);
            if (dir is null) return;

            var readmeFile = Directory.GetFiles(dir, "readme*", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
                                     f.EndsWith(".md", StringComparison.OrdinalIgnoreCase));

            if (readmeFile is not null)
            {
                var content = File.ReadAllText(readmeFile);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    ReadmeText.Text = content;
                    ReadmeSection.Visibility = Visibility.Visible;
                }
            }
        }
        catch { }
    }

    private void PrepareAnimation()
    {
        try
        {
            _smokeVisual = ElementCompositionPreview.GetElementVisual(SmokeLayer);
            _contentVisual = ElementCompositionPreview.GetElementVisual(ContentPanel);
            _heroVisual = ElementCompositionPreview.GetElementVisual(HeroSection);

            if (_smokeVisual is not null)
                _smokeVisual.Opacity = 0f;

            if (_contentVisual is not null)
            {
                _contentVisual.Opacity = 0f;
                _contentVisual.Scale = new System.Numerics.Vector3(0.92f, 0.92f, 1f);
            }

            if (_heroVisual is not null)
                _heroVisual.Opacity = 0f;
        }
        catch { }
    }

    private void PlayOpenAnimation()
    {
        try
        {
            var compositor = _smokeVisual?.Compositor;
            if (compositor is null) return;

            var ease = compositor.CreateCubicBezierEasingFunction(
                new System.Numerics.Vector2(0.16f, 1f),
                new System.Numerics.Vector2(0.3f, 1f));

            if (_smokeVisual is not null)
            {
                var smokeOpacity = compositor.CreateScalarKeyFrameAnimation();
                smokeOpacity.InsertKeyFrame(0f, 0f);
                smokeOpacity.InsertKeyFrame(1f, 1f, ease);
                smokeOpacity.Duration = TimeSpan.FromMilliseconds(350);
                _smokeVisual.StartAnimation("Opacity", smokeOpacity);
            }

            if (_contentVisual is not null)
            {
                var contentOpacity = compositor.CreateScalarKeyFrameAnimation();
                contentOpacity.InsertKeyFrame(0f, 0f);
                contentOpacity.InsertKeyFrame(1f, 1f, ease);
                contentOpacity.Duration = TimeSpan.FromMilliseconds(450);

                var contentScale = compositor.CreateVector3KeyFrameAnimation();
                contentScale.InsertKeyFrame(0f, new System.Numerics.Vector3(0.92f, 0.92f, 1f));
                contentScale.InsertKeyFrame(1f, new System.Numerics.Vector3(1f, 1f, 1f), ease);
                contentScale.Duration = TimeSpan.FromMilliseconds(500);

                _contentVisual.CenterPoint = new System.Numerics.Vector3(
                    (float)ContentPanel.ActualSize.X / 2,
                    (float)ContentPanel.ActualSize.Y / 3,
                    0f);

                _contentVisual.StartAnimation("Opacity", contentOpacity);
                _contentVisual.StartAnimation("Scale", contentScale);
            }

            if (_heroVisual is not null)
            {
                var heroEase = compositor.CreateCubicBezierEasingFunction(
                    new System.Numerics.Vector2(0.0f, 0.0f),
                    new System.Numerics.Vector2(1f, 1f));

                var heroOffset = compositor.CreateVector3KeyFrameAnimation();
                heroOffset.InsertKeyFrame(0f, new System.Numerics.Vector3(0, 40, 0));
                heroOffset.InsertKeyFrame(1f, new System.Numerics.Vector3(0, 0, 0), heroEase);
                heroOffset.Duration = TimeSpan.FromMilliseconds(550);

                var heroOpacity = compositor.CreateScalarKeyFrameAnimation();
                heroOpacity.InsertKeyFrame(0f, 0f);
                heroOpacity.InsertKeyFrame(1f, 1f, heroEase);
                heroOpacity.Duration = TimeSpan.FromMilliseconds(450);

                var heroScale = compositor.CreateVector3KeyFrameAnimation();
                heroScale.InsertKeyFrame(0f, new System.Numerics.Vector3(0.9f, 0.9f, 1f));
                heroScale.InsertKeyFrame(1f, new System.Numerics.Vector3(1f, 1f, 1f), heroEase);
                heroScale.Duration = TimeSpan.FromMilliseconds(550);

                _heroVisual.CenterPoint = new System.Numerics.Vector3(
                    (float)HeroSection.ActualSize.X / 2,
                    (float)HeroSection.ActualSize.Y / 2,
                    0f);

                _heroVisual.StartAnimation("Offset", heroOffset);
                _heroVisual.StartAnimation("Opacity", heroOpacity);
                _heroVisual.StartAnimation("Scale", heroScale);
            }
        }
        catch { }
    }

    private void PlayCloseAnimation()
    {
        if (_isClosing) return;
        _isClosing = true;

        try
        {
            var compositor = _smokeVisual?.Compositor;
            if (compositor is null || _popup is null)
            {
                _popup?.IsOpen = false;
                return;
            }

            var ease = compositor.CreateCubicBezierEasingFunction(
                new System.Numerics.Vector2(0.7f, 0f),
                new System.Numerics.Vector2(1f, 0.5f));

            var smokeOpacity = compositor.CreateScalarKeyFrameAnimation();
            smokeOpacity.InsertKeyFrame(0f, 1f);
            smokeOpacity.InsertKeyFrame(1f, 0f, ease);
            smokeOpacity.Duration = TimeSpan.FromMilliseconds(200);

            if (_contentVisual is not null)
            {
                var contentOpacity = compositor.CreateScalarKeyFrameAnimation();
                contentOpacity.InsertKeyFrame(0f, 1f);
                contentOpacity.InsertKeyFrame(1f, 0f, ease);
                contentOpacity.Duration = TimeSpan.FromMilliseconds(200);

                var contentScale = compositor.CreateVector3KeyFrameAnimation();
                contentScale.InsertKeyFrame(0f, new System.Numerics.Vector3(1f, 1f, 1f));
                contentScale.InsertKeyFrame(1f, new System.Numerics.Vector3(0.96f, 0.96f, 1f), ease);
                contentScale.Duration = TimeSpan.FromMilliseconds(250);

                _contentVisual.CenterPoint = new System.Numerics.Vector3(
                    (float)ContentPanel.ActualSize.X / 2,
                    (float)ContentPanel.ActualSize.Y / 3,
                    0f);

                _contentVisual.StartAnimation("Opacity", contentOpacity);
                _contentVisual.StartAnimation("Scale", contentScale);
            }

            if (_heroVisual is not null)
            {
                var heroOpacity = compositor.CreateScalarKeyFrameAnimation();
                heroOpacity.InsertKeyFrame(0f, 1f);
                heroOpacity.InsertKeyFrame(1f, 0f, ease);
                heroOpacity.Duration = TimeSpan.FromMilliseconds(200);

                var heroOffset = compositor.CreateVector3KeyFrameAnimation();
                heroOffset.InsertKeyFrame(0f, new System.Numerics.Vector3(0, 0, 0));
                heroOffset.InsertKeyFrame(1f, new System.Numerics.Vector3(0, 20, 0), ease);
                heroOffset.Duration = TimeSpan.FromMilliseconds(250);

                _heroVisual.StartAnimation("Opacity", heroOpacity);
                _heroVisual.StartAnimation("Offset", heroOffset);
            }

            if (_smokeVisual is not null)
                _smokeVisual.StartAnimation("Opacity", smokeOpacity);

            var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += (_, _) =>
            {
                if (_popup is not null)
                {
                    _popup.IsOpen = false;
                    _popup.Closed -= OnPopupClosed;
                }
                _isClosing = false;
            };
            batch.End();
        }
        catch
        {
            if (_popup is not null)
            {
                _popup.IsOpen = false;
                _popup.Closed -= OnPopupClosed;
            }
            _isClosing = false;
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        if (_tool is null) return;
        ToolLaunched?.Invoke(_tool);
        Close();
    }

    private void AdminButton_Click(object sender, RoutedEventArgs e)
    {
        if (_tool is null) return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _tool.Path,
                WorkingDirectory = Path.GetDirectoryName(_tool.Path) ?? ToolCatalog.ToolsRoot,
                UseShellExecute = true,
                Verb = "runAs"
            });
            ShowStatus("已以管理员身份启动", _tool.Name, InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            ShowStatus("启动失败", ex.Message, InfoBarSeverity.Error);
        }
    }

    private void FavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_tool is null) return;

        FavoritesService.ToggleFavorite(_tool.Path);
        _tool.IsFavorite = !_tool.IsFavorite;
        UpdateFavoriteIcon(_tool.IsFavorite);
        FavoriteChanged?.Invoke(_tool);
    }

    private void DesktopShortcutButton_Click(object sender, RoutedEventArgs e)
    {
        if (_tool is null) return;

        try
        {
            CreateDesktopShortcut(_tool);
            ShowStatus("已创建", $"已将「{_tool.Name}」快捷方式发送到桌面", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            ShowStatus("创建失败", ex.Message, InfoBarSeverity.Error);
        }
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (_tool is null) return;

        try
        {
            var dir = Path.GetDirectoryName(_tool.Path) ?? ToolCatalog.ToolsRoot;
            Process.Start(new ProcessStartInfo
            {
                FileName = dir,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        catch (Exception ex)
        {
            ShowStatus("打开失败", ex.Message, InfoBarSeverity.Error);
        }
    }

    private void UpdateFavoriteIcon(bool isFavorite)
    {
        FavoriteIcon.Glyph = isFavorite ? "\uE735" : "\uE734";
    }

    private void ShowStatus(string title, string message, InfoBarSeverity severity)
    {
        StatusBar.Title = title;
        StatusBar.Message = message;
        StatusBar.Severity = severity;
        StatusBar.IsOpen = true;
    }

    private static void CreateDesktopShortcut(ToolItem tool)
    {
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var shortcutPath = Path.Combine(desktop, $"{tool.Name}.lnk");

        var psScript = $"""
            $ws = New-Object -ComObject WScript.Shell
            $s = $ws.CreateShortcut('{shortcutPath}')
            $s.TargetPath = '{tool.Path}'
            $s.WorkingDirectory = '{Path.GetDirectoryName(tool.Path) ?? ToolCatalog.ToolsRoot}'
            $s.Description = '{tool.Name}'
            $s.Save()
            """;

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -NonInteractive -Command \"{psScript.Replace("\"", "\\\"")}\"",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(psi);
        process?.WaitForExit(5000);

        if (process is not null && process.ExitCode != 0)
        {
            var err = process.StandardError.ReadToEnd();
            throw new InvalidOperationException(err);
        }
    }

    private static string ValueOrUnknown(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "未知" : value;
    }
}