using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace SS14.Launcher.Models.ResourcePacks;

public sealed class ResourcePackManager
{
    private static readonly JsonSerializerOptions ConfigJsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly StringComparer _pathComparer;
    private readonly string _packsDirectory;
    private readonly string _configPath;
    private readonly string _overlayCacheDirectory;

    public ResourcePackManager(
        string? packsDirectory = null,
        string? configPath = null,
        string? overlayCacheDirectory = null)
    {
        _packsDirectory = Path.GetFullPath(packsDirectory ?? LauncherPaths.DirResourcePacks);
        _configPath = Path.GetFullPath(configPath ?? LauncherPaths.PathResourcePacksConfig);
        _overlayCacheDirectory = Path.GetFullPath(overlayCacheDirectory ?? LauncherPaths.DirResourcePackOverlayCache);
        _pathComparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    }

    public string PacksDirectory => _packsDirectory;

    public void Initialize()
    {
        Helpers.EnsureDirectoryExists(_packsDirectory);
        Helpers.EnsureDirectoryExists(_overlayCacheDirectory);
    }

    public IReadOnlyList<ResourcePackInfo> LoadPacks()
    {
        Initialize();

        var packs = new List<ResourcePackInfo>();
        foreach (var packDirectory in Directory.EnumerateDirectories(_packsDirectory).OrderBy(path => path, _pathComparer))
        {
            if (TryLoadPack(packDirectory, out var pack) && pack != null)
                packs.Add(pack);
        }

        return ApplySavedOrder(packs, LoadConfigEntries());
    }

    public void SavePacks(IEnumerable<ResourcePackInfo> packs)
    {
        Initialize();

        var config = packs
            .Select(pack => new ResourcePackConfigEntry(Path.GetFullPath(pack.DirectoryPath), pack.Enabled))
            .ToArray();

        var json = JsonSerializer.Serialize(config, ConfigJsonOptions);
        File.WriteAllText(_configPath, json, Encoding.UTF8);
    }

    public async Task<string?> BuildOverlayZipAsync(
        IEnumerable<ResourcePackInfo> packs,
        string? forkId,
        CancellationToken cancel = default)
    {
        Initialize();

        var overlayEntries = CollectOverlayEntries(packs, forkId);
        if (overlayEntries.Count == 0)
            return null;

        var cacheKey = ComputeCacheKey(overlayEntries);
        var overlayPath = Path.Combine(_overlayCacheDirectory, $"{cacheKey}.zip");
        if (File.Exists(overlayPath))
            return overlayPath;

        var tempPath = overlayPath + ".tmp";
        if (File.Exists(tempPath))
            File.Delete(tempPath);

        await Task.Run(() => CreateOverlayArchive(tempPath, overlayEntries, cancel), cancel);

        File.Move(tempPath, overlayPath, true);
        CleanupOverlayCache(overlayPath);
        return overlayPath;
    }

    internal IReadOnlyList<OverlayEntry> CollectOverlayEntries(IEnumerable<ResourcePackInfo> packs, string? forkId)
    {
        var selectedEntries = new Dictionary<string, OverlayEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var pack in packs.Where(pack => pack.Enabled && pack.AppliesTo(forkId)))
        {
            foreach (var entry in EnumeratePackEntries(pack))
            {
                selectedEntries.TryAdd(entry.EntryPath, entry);
            }
        }

        return selectedEntries.Values
            .OrderBy(entry => entry.EntryPath, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private bool TryLoadPack(string packDirectory, out ResourcePackInfo? pack)
    {
        pack = null;

        var metaPath = Path.Combine(packDirectory, "meta.json");
        if (!File.Exists(metaPath))
        {
            Log.Warning("Skipping resource pack without meta.json: {PackDirectory}", packDirectory);
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(metaPath, Encoding.UTF8));
            if (!TryGetRequiredString(document.RootElement, out var name, "name", "Name", "title") ||
                string.IsNullOrWhiteSpace(name))
            {
                Log.Warning("Skipping resource pack with missing name: {PackDirectory}", packDirectory);
                return false;
            }

            var description = GetOptionalString(document.RootElement, "description", "Description", "desc");
            var targetForkId = GetOptionalString(document.RootElement, "target", "Target");

            var iconPath = Path.Combine(packDirectory, "icon.png");
            if (!File.Exists(iconPath))
                iconPath = null;

            pack = new ResourcePackInfo(packDirectory, name, description, targetForkId, iconPath);
            return true;
        }
        catch (Exception exception)
        {
            Log.Warning(exception, "Skipping malformed resource pack metadata: {PackDirectory}", packDirectory);
            return false;
        }
    }

    private IReadOnlyList<ResourcePackInfo> ApplySavedOrder(
        IReadOnlyList<ResourcePackInfo> packs,
        IReadOnlyList<ResourcePackConfigEntry> configEntries)
    {
        var remaining = packs.ToDictionary(pack => Path.GetFullPath(pack.DirectoryPath), _pathComparer);
        var ordered = new List<ResourcePackInfo>(packs.Count);

        foreach (var entry in configEntries)
        {
            var normalizedPath = Path.GetFullPath(entry.DirectoryPath);
            if (!remaining.TryGetValue(normalizedPath, out var pack))
                continue;

            pack.Enabled = entry.Enabled;
            ordered.Add(pack);
            remaining.Remove(normalizedPath);
        }

        foreach (var pack in packs)
        {
            if (!remaining.ContainsKey(Path.GetFullPath(pack.DirectoryPath)))
                continue;

            ordered.Add(pack);
        }

        return ordered;
    }

    private IReadOnlyList<ResourcePackConfigEntry> LoadConfigEntries()
    {
        if (!File.Exists(_configPath))
            return Array.Empty<ResourcePackConfigEntry>();

        try
        {
            var json = File.ReadAllText(_configPath, Encoding.UTF8);
            return JsonSerializer.Deserialize<ResourcePackConfigEntry[]>(json, ConfigJsonOptions) ??
                   Array.Empty<ResourcePackConfigEntry>();
        }
        catch (Exception exception)
        {
            Log.Warning(exception, "Failed to load resource pack config from {ConfigPath}", _configPath);
            return Array.Empty<ResourcePackConfigEntry>();
        }
    }

    private IEnumerable<OverlayEntry> EnumeratePackEntries(ResourcePackInfo pack)
    {
        if (!Directory.Exists(pack.ResourcesPath))
            yield break;

        var topLevelEntries = Directory.EnumerateFileSystemEntries(pack.ResourcesPath).ToArray();
        foreach (var unsupportedEntryName in GetUnsupportedRootEntries(topLevelEntries))
        {
            Log.Warning(
                "Ignoring unsupported resource pack root {EntryName} in {PackDirectory}. Supported roots: {SupportedRoots}",
                unsupportedEntryName,
                pack.DirectoryPath,
                ResourcePackOverlayPolicy.AllowedRootsLabel);
        }

        foreach (var contentRootDirectory in GetSupportedRootDirectories(topLevelEntries))
        {
            foreach (var filePath in Directory.EnumerateFiles(contentRootDirectory, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(pack.ResourcesPath, filePath);
                if (!ResourcePackOverlayPolicy.TryNormalizePath(relativePath, out var normalizedPath))
                    continue;

                yield return new OverlayEntry(normalizedPath, filePath, pack.DirectoryPath);
            }
        }
    }

    private static IReadOnlyList<string> GetSupportedRootDirectories(IEnumerable<string> topLevelEntries)
    {
        return topLevelEntries
            .Where(path => Directory.Exists(path))
            .Where(path => ResourcePackOverlayPolicy.IsAllowedRoot(Path.GetFileName(path)))
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<string> GetUnsupportedRootEntries(IEnumerable<string> topLevelEntries)
    {
        return topLevelEntries
            .Select(path => new
            {
                Path = path,
                Name = Path.GetFileName(path)
            })
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Name))
            .Where(entry => !Directory.Exists(entry.Path) || !ResourcePackOverlayPolicy.IsAllowedRoot(entry.Name))
            .Select(entry => entry.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? GetOptionalString(JsonElement rootElement, params string[] propertyNames)
    {
        if (rootElement.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var property in rootElement.EnumerateObject())
        {
            if (!propertyNames.Any(name => string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)))
                continue;

            return property.Value.ValueKind == JsonValueKind.String
                ? property.Value.GetString()
                : property.Value.ToString();
        }

        return null;
    }

    private static bool TryGetRequiredString(JsonElement rootElement, out string value, params string[] propertyNames)
    {
        var maybeValue = GetOptionalString(rootElement, propertyNames);
        value = maybeValue ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

    private static string ComputeCacheKey(IEnumerable<OverlayEntry> overlayEntries)
    {
        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        foreach (var entry in overlayEntries)
        {
            var fileInfo = new FileInfo(entry.SourcePath);
            Append(hasher, entry.EntryPath);
            Append(hasher, entry.SourcePath);
            Append(hasher, fileInfo.Length.ToString());
            Append(hasher, fileInfo.LastWriteTimeUtc.Ticks.ToString());
        }

        return Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant();

        static void Append(IncrementalHash hasher, string value)
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(value));
            hasher.AppendData(stackalloc byte[] { 0 });
        }
    }

    private static void CreateOverlayArchive(
        string archivePath,
        IReadOnlyList<OverlayEntry> overlayEntries,
        CancellationToken cancel)
    {
        using var fileStream = new FileStream(archivePath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create);

        var buffer = ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            foreach (var entry in overlayEntries)
            {
                cancel.ThrowIfCancellationRequested();

                var archiveEntry = archive.CreateEntry(entry.EntryPath, CompressionLevel.Optimal);
                using var sourceStream = new FileStream(entry.SourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var targetStream = archiveEntry.Open();

                while (true)
                {
                    cancel.ThrowIfCancellationRequested();
                    var bytesRead = sourceStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;

                    targetStream.Write(buffer, 0, bytesRead);
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private void CleanupOverlayCache(string preservedArchivePath)
    {
        foreach (var cachedArchive in Directory.EnumerateFiles(_overlayCacheDirectory, "*.zip"))
        {
            if (_pathComparer.Equals(cachedArchive, preservedArchivePath))
                continue;

            try
            {
                File.Delete(cachedArchive);
            }
            catch (Exception exception)
            {
                Log.Debug(exception, "Failed to remove stale resource pack overlay cache file: {ArchivePath}", cachedArchive);
            }
        }
    }

    internal sealed record OverlayEntry(string EntryPath, string SourcePath, string PackDirectory);

    private sealed record ResourcePackConfigEntry(string DirectoryPath, bool Enabled);
}
