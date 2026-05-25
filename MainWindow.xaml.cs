using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TubaWinUi3.Pages;
using TubaWinUi3.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TubaWinUi3;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        AppWindow.SetIcon("Assets/AppIcon.ico");

        PopulateCategories();
        NavFrame.Navigate(typeof(HomePage), null);
    }

    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    private void TitleBar_BackRequested(TitleBar sender, object args)
    {
        NavFrame.GoBack();
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            NavFrame.Navigate(typeof(SettingsPage));
        }
        else if (args.SelectedItem is NavigationViewItem item)
        {
            switch (item.Tag)
            {
                case "all":
                    NavFrame.Navigate(typeof(HomePage), null);
                    break;
                case "favorites":
                    NavFrame.Navigate(typeof(FavoritesPage));
                    break;
                case "hardware":
                    NavFrame.Navigate(typeof(HardwarePage));
                    break;
                case "builtin":
                    NavFrame.Navigate(typeof(BuiltinToolsPage));
                    break;
                case string category:
                    NavFrame.Navigate(typeof(HomePage), category);
                    break;
            }
        }

        ThemeService.ApplySavedTheme();
    }

    private void PopulateCategories()
    {
        foreach (var category in ToolCatalog.GetCategories())
        {
            NavView.MenuItems.Add(new NavigationViewItem
            {
                Content = category,
                Tag = category,
                Icon = new FontIcon { Glyph = GetCategoryGlyph(category) }
            });
        }
    }

    private static string GetCategoryGlyph(string category)
    {
        if (category.Contains("处理器", StringComparison.CurrentCultureIgnoreCase))
        {
            return "\uE950";
        }

        if (category.Contains("显卡", StringComparison.CurrentCultureIgnoreCase) ||
            category.Contains("显示器", StringComparison.CurrentCultureIgnoreCase))
        {
            return "\uE7F4";
        }

        if (category.Contains("硬盘", StringComparison.CurrentCultureIgnoreCase))
        {
            return "\uEDA2";
        }

        if (category.Contains("内存", StringComparison.CurrentCultureIgnoreCase))
        {
            return "\uE8B9";
        }

        if (category.Contains("游戏", StringComparison.CurrentCultureIgnoreCase))
        {
            return "\uE7FC";
        }

        if (category.Contains("烤鸡", StringComparison.CurrentCultureIgnoreCase))
        {
            return "\uE945";
        }

        if (category.Contains("声卡", StringComparison.CurrentCultureIgnoreCase))
        {
            return "\uEA69";
        }

        return "\uE8B7";
    }
}
