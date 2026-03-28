using System;
using System.IO;

namespace SS14.Launcher.Models.ResourcePacks;

public sealed class ResourcePackInfo
{
    public ResourcePackInfo(
        string directoryPath,
        string name,
        string? description,
        string? targetForkId,
        string? iconPath)
    {
        DirectoryPath = Path.GetFullPath(directoryPath);
        Name = name;
        Description = description ?? string.Empty;
        TargetForkId = targetForkId ?? string.Empty;
        IconPath = iconPath;
    }

    public string DirectoryPath { get; }
    public string Name { get; }
    public string Description { get; }
    public string TargetForkId { get; }
    public string? IconPath { get; }

    public bool Enabled { get; set; } = true;

    public string DirectoryName => Path.GetFileName(DirectoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

    public string ResourcesPath => Path.Combine(DirectoryPath, "Resources");

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    public bool HasTargetForkId => !string.IsNullOrWhiteSpace(TargetForkId);

    public bool AppliesTo(string? forkId)
    {
        if (string.IsNullOrWhiteSpace(TargetForkId))
            return true;

        return !string.IsNullOrWhiteSpace(forkId) &&
               string.Equals(TargetForkId, forkId, StringComparison.OrdinalIgnoreCase);
    }
}
