using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.Graphics;
using Windows.UI;

namespace TubaWinUi3.Services;

public sealed class DiskSpaceAnalyzerTool : IBuiltinTool
{
    public string Id => "disk-space-analyzer";
    public string Name => "磁盘分析";
    public string Description => "可视化磁盘空间占用，以树状图展示文件夹大小，类似 SpaceSniffer。";
    public string Glyph => "\uEDA2";
    public string Category => "系统工具";
    public BuiltinToolKind Kind => BuiltinToolKind.Dialog;

    public async Task ExecuteAsync(BuiltinToolContext context)
    {
        var drives = DriveInfo.GetDrives()
            .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
            .ToList();

        if (drives.Count == 0)
        {
            var d = new ContentDialog { Title = "磁盘分析", Content = "未检测到可用磁盘。", CloseButtonText = "关闭", XamlRoot = context.XamlRoot };
            await d.ShowAsync();
            return;
        }

        if (drives.Count == 1) { OpenWindow(drives[0].RootDirectory.FullName); return; }

        var sp = new StackPanel { Spacing = 12, Padding = new Thickness(4) };
        sp.Children.Add(new TextBlock { Text = "选择要分析的磁盘：", FontSize = 14, Opacity = 0.8 });
        var list = new StackPanel { Spacing = 8 };
        foreach (var drive in drives)
        {
            var txt = $"{drive.Name}  {Fmt(drive.AvailableFreeSpace)} 可用 / {Fmt(drive.TotalSize)} 总计";
            var btn = new Button { Content = new TextBlock { Text = txt, FontSize = 13 }, HorizontalAlignment = HorizontalAlignment.Stretch, Padding = new Thickness(16, 10, 16, 10), Tag = drive.RootDirectory.FullName };
            btn.Click += (s, _) => { if (s is Button b) OpenWindow((string)b.Tag); };
            list.Children.Add(btn);
        }
        sp.Children.Add(list);
        var dlg = new ContentDialog { Title = "磁盘分析", Content = sp, CloseButtonText = "取消", XamlRoot = context.XamlRoot };
        dlg.Resources["ContentDialogMaxWidth"] = 500;
        await dlg.ShowAsync();
    }

    private static void OpenWindow(string path)
    {
        var w = new Window();
        w.AppWindow.Title = "磁盘分析";
        w.AppWindow.Resize(new SizeInt32(1000, 700));
        w.AppWindow.Move(new PointInt32(60, 60));
        w.Content = new AnalyzerPage(path, w);
        w.Activate();
    }

    internal static string Fmt(long bytes)
    {
        string[] u = ["B", "KB", "MB", "GB", "TB"];
        double s = bytes; int i = 0;
        while (s >= 1024 && i < u.Length - 1) { s /= 1024; i++; }
        return $"{s:0.#} {u[i]}";
    }
}

file sealed class AnalyzerPage : Page
{
    private readonly Window _win;
    private TNode? _root;
    private TNode? _cur;
    private readonly Stack<TNode> _nav = [];
    private Canvas _cv = null!;
    private TextBlock _bc = null!;
    private TextBlock _st = null!;
    private TextBlock _tip = null!;
    private ProgressBar _pb = null!;
    private Border _tipBox = null!;
    private CancellationTokenSource? _cts;
    private long _pBytes;
    private int _pItems;
    private TNode? _hoveredNode;

    private static readonly SolidColorBrush NormalBorder = new(Color.FromArgb(40, 0, 0, 0));
    private static readonly SolidColorBrush HoverBorder = new(Color.FromArgb(230, 255, 255, 255));
    private static readonly SolidColorBrush TipBg = new(Color.FromArgb(235, 40, 40, 40));
    private static readonly SolidColorBrush TipBorder = new(Color.FromArgb(255, 100, 100, 100));
    private static readonly SolidColorBrush TipFg = new(Color.FromArgb(255, 255, 255, 255));
    private static readonly SolidColorBrush CanvasBg = new(Color.FromArgb(255, 20, 20, 20));
    private static readonly SolidColorBrush LabelMain = new(Color.FromArgb(235, 255, 255, 255));
    private static readonly SolidColorBrush LabelSub = new(Color.FromArgb(175, 255, 255, 255));

    private static readonly Color[] Palette =
    [
        Color.FromArgb(255, 70, 130, 230), Color.FromArgb(255, 50, 170, 80),
        Color.FromArgb(255, 230, 70, 55),  Color.FromArgb(255, 245, 185, 10),
        Color.FromArgb(255, 165, 75, 185), Color.FromArgb(255, 10, 170, 195),
        Color.FromArgb(255, 250, 110, 65), Color.FromArgb(255, 80, 175, 85),
        Color.FromArgb(255, 120, 135, 200), Color.FromArgb(255, 250, 165, 40),
    ];

    public AnalyzerPage(string path, Window win)
    {
        _win = win;
        InitUi();
        _ = ScanAsync(path);
    }

    private void InitUi()
    {
        var bk = new Button { Content = new FontIcon { Glyph = "\uE72B", FontSize = 14 }, Padding = new Thickness(8, 4, 8, 4), VerticalAlignment = VerticalAlignment.Center };
        bk.Click += (_, _) => GoBack();
        _bc = new TextBlock { FontSize = 13, VerticalAlignment = VerticalAlignment.Center, Opacity = 0.85, TextTrimming = TextTrimming.CharacterEllipsis };
        var rf = new Button { Content = new FontIcon { Glyph = "\uE72C", FontSize = 14 }, Padding = new Thickness(8, 4, 8, 4), VerticalAlignment = VerticalAlignment.Center };
        rf.Click += (_, _) => { if (_cur != null) _ = ScanAsync(_cur.Path, true); };

        var top = new Grid { ColumnSpacing = 8, Padding = new Thickness(12, 8, 12, 4) };
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        top.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        top.Children.Add(bk);
        Grid.SetColumn(_bc, 1); top.Children.Add(_bc);
        Grid.SetColumn(rf, 2); top.Children.Add(rf);

        _cv = new Canvas { Background = CanvasBg };
        _cv.SizeChanged += (_, _) => Render();
        _cv.PointerPressed += OnClick;
        _cv.PointerMoved += OnMove;

        _pb = new ProgressBar { IsIndeterminate = true, Visibility = Visibility.Collapsed, Margin = new Thickness(0, 2, 0, 2) };
        _st = new TextBlock { FontSize = 12, Opacity = 0.7, Padding = new Thickness(12, 2, 12, 4) };

        _tip = new TextBlock { FontSize = 12, Foreground = TipFg };
        _tipBox = new Border { Background = TipBg, BorderBrush = TipBorder, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4), Padding = new Thickness(8, 5, 8, 5), Child = _tip, Visibility = Visibility.Collapsed };

        var wrap = new Grid();
        wrap.Children.Add(_cv);
        var tipCanvas = new Canvas { IsHitTestVisible = false };
        tipCanvas.Children.Add(_tipBox);
        wrap.Children.Add(tipCanvas);

        var g = new Grid();
        g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        g.Children.Add(top);
        Grid.SetRow(_pb, 1); g.Children.Add(_pb);
        Grid.SetRow(wrap, 2); g.Children.Add(wrap);
        Grid.SetRow(_st, 3); g.Children.Add(_st);
        Content = g;
    }

    private async Task ScanAsync(string path, bool keepNav = false)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var tk = _cts.Token;

        _pb.Visibility = Visibility.Visible;
        _st.Text = "正在扫描…";
        _cv.Children.Clear();
        _hoveredNode = null;
        _pBytes = 0; _pItems = 0;

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        timer.Tick += (_, _) => { _st.Text = $"正在扫描…  {_pItems:N0} 项  ·  {DiskSpaceAnalyzerTool.Fmt(_pBytes)}"; };
        timer.Start();

        TNode node;
        try
        {
            var opts = new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = false, ReturnSpecialDirectories = false };
            node = await Task.Run(() => BuildTree(path, opts, tk), tk);
        }
        catch (OperationCanceledException)
        {
            timer.Stop(); _pb.Visibility = Visibility.Collapsed; _st.Text = "扫描已取消"; return;
        }

        timer.Stop(); _pb.Visibility = Visibility.Collapsed;
        _root = node;
        _cur = node;
        if (!keepNav) _nav.Clear();
        SyncUi();
        _win.AppWindow.Title = $"磁盘分析 - {path}";
        Render();
    }

    private TNode BuildTree(string path, EnumerationOptions opts, CancellationToken tk)
    {
        tk.ThrowIfCancellationRequested();
        var name = System.IO.Path.GetFileName(path);
        if (string.IsNullOrEmpty(name)) name = path.TrimEnd('\\');
        var node = new TNode(name, path);
        long size = 0;

        try
        {
            foreach (var f in new DirectoryInfo(path).EnumerateFiles("*", opts))
            {
                tk.ThrowIfCancellationRequested();
                try { size += f.Length; node.FileCount++; Interlocked.Increment(ref _pItems); Interlocked.Add(ref _pBytes, f.Length); }
                catch { }
            }
        }
        catch { }

        try
        {
            foreach (var d in new DirectoryInfo(path).EnumerateDirectories("*", opts))
            {
                tk.ThrowIfCancellationRequested();
                try
                {
                    var child = BuildTree(d.FullName, opts, tk);
                    if (child.Size > 0) node.Children.Add(child);
                    node.DirCount++;
                    node.FileCount += child.FileCount;
                    node.DirCount += child.DirCount;
                    size += child.Size;
                }
                catch { }
            }
        }
        catch { }

        node.Size = size;
        node.Children.Sort((a, b) => b.Size.CompareTo(a.Size));
        return node;
    }

    private TNode? FindNodeAt(Point p)
    {
        foreach (var c in _cv.Children)
        {
            if (c is not Border b || b.Tag is not TNode n) continue;
            var x = Canvas.GetLeft(b); var y = Canvas.GetTop(b);
            if (p.X >= x && p.X <= x + b.Width && p.Y >= y && p.Y <= y + b.Height) return n;
        }
        return null;
    }

    private void OnClick(object sender, PointerRoutedEventArgs e)
    {
        if (!e.GetCurrentPoint(_cv).Properties.IsLeftButtonPressed) return;
        var node = FindNodeAt(e.GetCurrentPoint(_cv).Position);
        if (node != null) DrillIn(node);
    }

    private void OnMove(object sender, PointerRoutedEventArgs e)
    {
        var node = FindNodeAt(e.GetCurrentPoint(_cv).Position);

        if (node != _hoveredNode)
        {
            _hoveredNode = node;

            foreach (var c in _cv.Children)
            {
                if (c is not Border b) continue;
                if (b.Tag == node) { b.BorderBrush = HoverBorder; b.BorderThickness = new Thickness(1.5); }
                else { b.BorderBrush = NormalBorder; b.BorderThickness = new Thickness(0.5); }
            }

            if (node != null)
            {
                var pct = _cur != null ? (double)node.Size / _cur.Size * 100 : 0;
                _tip.Text = $"{node.Name}  ·  {DiskSpaceAnalyzerTool.Fmt(node.Size)}  ·  {pct:0.#}%  ·  {node.FileCount} 文件";
                _tipBox.Visibility = Visibility.Visible;
            }
            else
            {
                _tipBox.Visibility = Visibility.Collapsed;
            }
        }

        if (_tipBox.Visibility == Visibility.Visible)
        {
            var p = e.GetCurrentPoint(_cv).Position;
            Canvas.SetLeft(_tipBox, Math.Max(4, Math.Min(p.X + 14, _cv.ActualWidth - 260)));
            Canvas.SetTop(_tipBox, Math.Max(4, Math.Min(p.Y + 14, _cv.ActualHeight - 40)));
        }
    }

    private void DrillIn(TNode node)
    {
        if (node.DirCount == 0 && node.FileCount == 0 && node.Children.Count == 0) return;
        if (_cur != null) _nav.Push(_cur);
        _cur = node;
        _hoveredNode = null;
        SyncUi();
        _win.AppWindow.Title = $"磁盘分析 - {node.Path}";
        Render();
    }

    private void GoBack()
    {
        if (_nav.Count == 0) return;
        _cur = _nav.Pop();
        _hoveredNode = null;
        SyncUi();
        _win.AppWindow.Title = $"磁盘分析 - {_cur.Path}";
        Render();
    }

    private void SyncUi()
    {
        if (_cur == null) return;
        var parts = new List<string>();
        foreach (var n in _nav) parts.Add(n.Name);
        parts.Add(_cur.Name);
        _bc.Text = string.Join(" › ", parts);
        _st.Text = $"{DiskSpaceAnalyzerTool.Fmt(_cur.Size)}  ·  {_cur.FileCount:N0} 文件  ·  {_cur.DirCount:N0} 文件夹";
    }

    private void Render()
    {
        _cv.Children.Clear();
        _hoveredNode = null;
        if (_cur == null || _cur.Size == 0) return;

        var W = _cv.ActualWidth;
        var H = _cv.ActualHeight;
        if (W <= 0 || H <= 0) return;

        const double gap = 2;
        var items = new List<(TNode Node, double Ratio)>();
        foreach (var c in _cur.Children) items.Add((c, (double)c.Size / _cur.Size));
        if (items.Count == 0) return;

        var rects = DoSquarify(items, new Rect(gap, gap, W - gap * 2, H - gap * 2));

        foreach (var (node, rect) in rects)
        {
            if (rect.Width < 1.5 || rect.Height < 1.5) continue;

            var color = NodeColor(node);
            var brd = new Border
            {
                Tag = node,
                Background = new SolidColorBrush(color),
                BorderBrush = NormalBorder,
                BorderThickness = new Thickness(0.5)
            };

            var big = rect.Width > 70 && rect.Height > 36;
            var med = rect.Width > 44 && rect.Height > 22;

            if (med)
            {
                var sp = new StackPanel { Margin = new Thickness(3, 2, 3, 2) };
                sp.Children.Add(new TextBlock { Text = node.Name, FontSize = big ? 12 : 10, Foreground = LabelMain, TextTrimming = TextTrimming.CharacterEllipsis, TextWrapping = TextWrapping.NoWrap });
                sp.Children.Add(new TextBlock { Text = DiskSpaceAnalyzerTool.Fmt(node.Size), FontSize = big ? 10 : 9, Foreground = LabelSub });
                brd.Child = sp;
            }

            Canvas.SetLeft(brd, rect.X);
            Canvas.SetTop(brd, rect.Y);
            brd.Width = rect.Width;
            brd.Height = rect.Height;
            _cv.Children.Add(brd);
        }
    }

    private static List<(TNode Node, Rect Rect)> DoSquarify(List<(TNode Node, double Ratio)> items, Rect bounds)
    {
        var result = new List<(TNode Node, Rect Rect)>();
        if (items.Count == 0) return result;
        var totalArea = bounds.Width * bounds.Height;
        var remaining = items.Select(it => (it.Node, Area: it.Ratio * totalArea)).ToList();
        LayRow(remaining, bounds, result);
        return result;
    }

    private static void LayRow(List<(TNode Node, double Area)> items, Rect bounds, List<(TNode Node, Rect Rect)> result)
    {
        if (items.Count == 0) return;
        if (items.Count == 1) { result.Add((items[0].Node, bounds)); return; }

        var shortSide = Math.Min(bounds.Width, bounds.Height);
        if (shortSide <= 0) return;

        var row = new List<(TNode Node, double Area)> { items[0] };
        var bestW = WorstAspect(row, shortSide);

        for (int i = 1; i < items.Count; i++)
        {
            var test = new List<(TNode Node, double Area)>(row) { items[i] };
            var tw = WorstAspect(test, shortSide);
            if (tw <= bestW) { row.Add(items[i]); bestW = tw; }
            else
            {
                EmitRow(row, bounds, result);
                var rowArea = row.Sum(r => r.Area);
                var totalArea = bounds.Width * bounds.Height;
                var frac = rowArea / totalArea;
                var wide = bounds.Width >= bounds.Height;
                var nb = wide
                    ? new Rect(bounds.X + bounds.Width * frac, bounds.Y, bounds.Width * (1 - frac), bounds.Height)
                    : new Rect(bounds.X, bounds.Y + bounds.Height * frac, bounds.Width, bounds.Height * (1 - frac));
                LayRow(items.Skip(i).ToList(), nb, result);
                return;
            }
        }
        EmitRow(row, bounds, result);
    }

    private static void EmitRow(List<(TNode Node, double Area)> row, Rect bounds, List<(TNode Node, Rect Rect)> result)
    {
        var rowArea = row.Sum(r => r.Area);
        if (rowArea <= 0) return;
        var wide = bounds.Width >= bounds.Height;
        var rowLen = wide ? rowArea / bounds.Height : rowArea / bounds.Width;
        if (rowLen <= 0) return;

        var off = 0.0;
        foreach (var (node, area) in row)
        {
            if (area <= 0) continue;
            Rect r;
            if (wide) { var h = area / rowLen; r = new Rect(bounds.X, bounds.Y + off, rowLen, h); off += h; }
            else { var w = area / rowLen; r = new Rect(bounds.X + off, bounds.Y, w, rowLen); off += w; }
            result.Add((node, r));
        }
    }

    private static double WorstAspect(List<(TNode Node, double Area)> row, double side)
    {
        if (row.Count == 0 || side <= 0) return double.MaxValue;
        var total = row.Sum(r => r.Area);
        if (total <= 0) return double.MaxValue;
        var rowLen = total / side;
        if (rowLen <= 0) return double.MaxValue;
        var worst = 0.0;
        foreach (var (_, area) in row)
        {
            if (area <= 0) continue;
            var other = area / rowLen;
            worst = Math.Max(worst, Math.Max(rowLen / other, other / rowLen));
        }
        return worst;
    }

    private static Color NodeColor(TNode node)
    {
        var h = 0;
        foreach (var c in node.Name) h = (h * 31 + c) & 0x7FFFFFFF;
        var baseC = Palette[h % Palette.Length];
        var sf = Math.Min(1.0, Math.Log10(Math.Max(node.Size, 1)) / 10.5);
        var br = 0.38 + sf * 0.62;
        return Color.FromArgb(255, (byte)Math.Clamp(baseC.R * br, 0, 255), (byte)Math.Clamp(baseC.G * br, 0, 255), (byte)Math.Clamp(baseC.B * br, 0, 255));
    }
}

file sealed class TNode
{
    public string Name { get; }
    public string Path { get; }
    public long Size { get; set; }
    public int FileCount { get; set; }
    public int DirCount { get; set; }
    public List<TNode> Children { get; } = [];
    public TNode(string name, string path) { Name = name; Path = path; }
}