using Microsoft.UI.Xaml;

namespace TubaWinUi3.Services;

public static class ThemeService
{
    private const string Key = "AppTheme";
    private static AppTheme _currentTheme = AppTheme.Default;

    public static AppTheme CurrentTheme => _currentTheme;

    public static void SetTheme(AppTheme theme)
    {
        _currentTheme = theme;

        var settings = GetSettings();
        if (settings is not null)
            settings.Values[Key] = theme.ToString();

        ApplyTheme(theme);
    }

    public static void ApplySavedTheme()
    {
        var settings = GetSettings();
        if (settings is not null && settings.Values[Key] is string s && Enum.TryParse<AppTheme>(s, out var theme))
            _currentTheme = theme;

        ApplyTheme(_currentTheme);
    }

    private static void ApplyTheme(AppTheme theme)
    {
        var window = App.MainWindow;
        if (window?.Content is not FrameworkElement root)
            return;

        root.RequestedTheme = theme switch
        {
            AppTheme.Light => ElementTheme.Light,
            AppTheme.Dark => ElementTheme.Dark,
            _ => ElementTheme.Default
        };
    }

    private static Windows.Storage.ApplicationDataContainer? GetSettings()
    {
        try { return Windows.Storage.ApplicationData.Current.LocalSettings; }
        catch { return null; }
    }
}

public enum AppTheme
{
    Default,
    Light,
    Dark
}
