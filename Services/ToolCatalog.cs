using TubaWinUi3.Models;

namespace TubaWinUi3.Services;

public static class ToolCatalog
{
    private static readonly string[] LaunchableExtensions =
    [
        ".exe",
        ".bat",
        ".cmd",
        ".lnk",
        ".msc",
        ".ps1",
        ".vbs"
    ];

    public static string ToolsRoot => FindToolsRoot();

    public static IReadOnlyList<string> GetCategories()
    {
        if (!Directory.Exists(ToolsRoot))
        {
            return [];
        }

        return Directory.GetDirectories(ToolsRoot)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
            .Cast<string>()
            .ToList();
    }

    public static IReadOnlyList<ToolItem> GetTools(string? category)
    {
        if (string.IsNullOrWhiteSpace(category) || !Directory.Exists(ToolsRoot))
        {
            return [];
        }

        var categoryRoot = Path.Combine(ToolsRoot, category);
        if (!Directory.Exists(categoryRoot))
        {
            return [];
        }

        return Directory.EnumerateFiles(categoryRoot, "*", SearchOption.AllDirectories)
            .Where(IsLaunchable)
            .Select(path => CreateToolItem(category, categoryRoot, path))
            .OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.RelativePath, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<ToolItem> GetAllToolsLazy(int skip, int take)
    {
        if (!Directory.Exists(ToolsRoot))
            return [];

        return GetCategories()
            .SelectMany(GetTools)
            .Skip(skip)
            .Take(take)
            .ToList();
    }

    public static int GetAllToolsCount()
    {
        if (!Directory.Exists(ToolsRoot))
            return 0;

        return GetCategories()
            .Sum(c => GetTools(c).Count);
    }

    public static IReadOnlyList<ToolItem> Search(string query)
    {
        if (!Directory.Exists(ToolsRoot))
        {
            return [];
        }

        var normalizedQuery = query.Trim();
        if (normalizedQuery.Length == 0)
        {
            return [];
        }

        return GetCategories()
            .SelectMany(GetTools)
            .Where(item =>
                item.Name.Contains(normalizedQuery, StringComparison.CurrentCultureIgnoreCase) ||
                item.RelativePath.Contains(normalizedQuery, StringComparison.CurrentCultureIgnoreCase))
            .ToList();
    }

    private static ToolItem CreateToolItem(string category, string categoryRoot, string path)
    {
        var extension = Path.GetExtension(path);
        var name = GetDisplayName(path);
        var relativePath = Path.GetRelativePath(categoryRoot, path);
        var metadata = ToolMetadataService.GetMetadata(category, path);

        return new ToolItem
        {
            Name = CleanupName(name),
            Category = category,
            Path = path,
            RelativePath = relativePath,
            Extension = extension.TrimStart('.').ToUpperInvariant(),
            IconPath = ToolIconService.GetIconPath(path),
            Description = metadata.Description,
            Publisher = metadata.Publisher,
            Version = metadata.Version,
            DatabaseSource = metadata.DatabaseSource,
            IsFavorite = FavoritesService.IsFavorite(path)
        };
    }

    private static bool IsLaunchable(string path)
    {
        var extension = Path.GetExtension(path);
        return LaunchableExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    private static string CleanupName(string name)
    {
        return name
            .Replace("_x64", " x64", StringComparison.OrdinalIgnoreCase)
            .Replace("_x86", " x86", StringComparison.OrdinalIgnoreCase)
            .Replace("_", " ");
    }

    private static string GetDisplayName(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        if (!fileName.Equals("start", StringComparison.OrdinalIgnoreCase))
        {
            return fileName;
        }

        var parentName = Directory.GetParent(path)?.Name;
        return string.IsNullOrWhiteSpace(parentName) ? fileName : parentName;
    }

    private static string FindToolsRoot()
    {
        var outputTools = Path.Combine(AppContext.BaseDirectory, "Tools");
        if (Directory.Exists(outputTools))
        {
            return outputTools;
        }

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "Tools");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return outputTools;
    }
}
