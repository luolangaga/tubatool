using Microsoft.UI.Xaml.Controls;
using TubaWinUi3.Pages;

namespace TubaWinUi3.Services;

public sealed class LiteMonitorTool : IBuiltinTool
{
    public string Id => "lite-monitor";
    public string Name => "硬件监控";
    public string Description => "实时监控 CPU/GPU/内存/磁盘/网络与帧率，支持悬窗置顶显示。";
    public string Glyph => "\uE945";
    public string Category => "监测工具";
    public BuiltinToolKind Kind => BuiltinToolKind.Dialog;

    public async Task ExecuteAsync(BuiltinToolContext context)
    {
        var service = LiteMonitorService.Instance;

        if (!LiteMonitorService.IsDriverReady())
        {
            var driverOk = await service.EnsureDriverAsync(context.XamlRoot);
            if (!driverOk) return;
        }

        var dialog = new ContentDialog
        {
            Title = "硬件监控",
            CloseButtonText = "关闭",
            XamlRoot = context.XamlRoot
        };
        dialog.Resources["ContentDialogMaxWidth"] = 960;

        var page = new LiteMonitorPage();
        dialog.Content = page;

        await dialog.ShowAsync();
    }
}