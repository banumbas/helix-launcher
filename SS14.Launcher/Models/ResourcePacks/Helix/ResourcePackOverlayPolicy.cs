using System;
using System.Collections.Generic;
using System.Linq;

namespace SS14.Launcher.Models.ResourcePacks;

public static class ResourcePackOverlayPolicy
{
    private static readonly string[] AllowedRoots =
    [
        "Audio",
        "Fonts",
        "Locale",
        "Shaders",
        "Textures"
    ];

    private static readonly HashSet<string> AllowedRootSet = new(AllowedRoots, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<string> AllowedRootNames => AllowedRoots;

    public static string AllowedRootsLabel => string.Join(", ", AllowedRoots);

    public static bool IsAllowedRoot(string? rootName)
    {
        return !string.IsNullOrWhiteSpace(rootName) &&
               AllowedRootSet.Contains(rootName);
    }

    public static bool TryNormalizePath(string path, out string normalizedPath)
    {
        normalizedPath = string.Empty;

        if (string.IsNullOrWhiteSpace(path))
            return false;

        var segments = path
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length == 0)
            return false;

        if (segments.Any(segment => segment is "." or ".."))
            return false;

        normalizedPath = string.Join("/", segments);
        return true;
    }

    public static bool IsAllowedPath(string path)
    {
        if (!TryNormalizePath(path, out var normalizedPath))
            return false;

        var separatorIndex = normalizedPath.IndexOf('/');
        var rootName = separatorIndex == -1
            ? normalizedPath
            : normalizedPath[..separatorIndex];

        return IsAllowedRoot(rootName);
    }
}
