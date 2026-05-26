using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Reflection;
using TubaWinUi3.Services;

namespace TubaWinUi3.Pages;

public sealed partial class SettingsPage : Page
{
    private bool _isCheckingUpdate;

    public SettingsPage()
    {
        InitializeComponent();

        ToolSourceText.Text = "本软件所集成的全部硬件检测与测试工具，均来源于\u201C图吧工具箱\u201D项目。感谢图吧工具箱社区对 PC 硬件工具生态的贡献。";

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = version is not null
            ? $"版本 {version.Major}.{version.Minor}.{version.Build}"
            : "版本 1.0.0";

        LoadGitHubAvatar();
        InitThemeComboBox();
    }

    private void LoadGitHubAvatar()
    {
        try
        {
            AuthorAvatar.ProfilePicture = new BitmapImage(new Uri("https://github.com/luolangaga.png"));
        }
        catch
        {
        }
    }

    private void InitThemeComboBox()
    {
        ThemeComboBox.Items.Add("跟随系统");
        ThemeComboBox.Items.Add("浅色");
        ThemeComboBox.Items.Add("深色");
        ThemeComboBox.SelectedIndex = ThemeService.CurrentTheme switch
        {
            AppTheme.Light => 1,
            AppTheme.Dark => 2,
            _ => 0
        };
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var theme = ThemeComboBox.SelectedIndex switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.Default
        };
        ThemeService.SetTheme(theme);
    }

    private async void CheckUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isCheckingUpdate) return;
        _isCheckingUpdate = true;
        CheckUpdateButton.IsEnabled = false;
        UpdateStatusText.Text = "正在检查更新...";

        try
        {
            var update = await UpdateService.CheckForUpdateAsync();

            if (update is not null)
            {
                UpdateStatusText.Text = $"发现新版本 v{update.Version}";
                var dialog = new UpdateDialog();
                await dialog.ShowUpdateAsync(update);

                if (dialog.SkipThisVersion)
                    UpdateStatusText.Text = $"已跳过 v{update.Version}";
                else
                    UpdateStatusText.Text = "点击检查是否有新版本";
            }
            else
            {
                UpdateStatusText.Text = "已是最新版本";
            }
        }
        catch (Exception ex)
        {
            UpdateStatusText.Text = $"检查失败: {ex.Message}";
        }
        finally
        {
            _isCheckingUpdate = false;
            CheckUpdateButton.IsEnabled = true;
        }
    }

    private void ThrowErrorButton_Click(object sender, RoutedEventArgs e)
    {
        throw new InvalidOperationException("这是一条手动抛出的测试异常，用于验证全局错误页面是否正常工作。");
    }
}
