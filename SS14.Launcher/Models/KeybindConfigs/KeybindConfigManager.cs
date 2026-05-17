using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Serilog;

namespace SS14.Launcher.Models.KeybindConfigs;

public sealed class KeybindConfigManager
{
    private static readonly JsonSerializerOptions ConfigJsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly StringComparer _pathComparer;
    private readonly string _configsDirectory;
    private readonly string _configPath;
    private readonly string _clientKeybindsPath;
    private readonly string? _legacyConfigsDirectory;
    private readonly string? _legacyConfigPath;

    public KeybindConfigManager(
        string? configsDirectory = null,
        string? configPath = null,
        string? clientKeybindsPath = null,
        string? legacyConfigsDirectory = null,
        string? legacyConfigPath = null)
    {
        _configsDirectory = Path.GetFullPath(configsDirectory ?? LauncherPaths.DirKeybindConfigs);
        _configPath = Path.GetFullPath(configPath ?? LauncherPaths.PathKeybindConfigsConfig);
        _clientKeybindsPath = Path.GetFullPath(clientKeybindsPath ?? LauncherPaths.PathClientKeybinds);
        _legacyConfigsDirectory = legacyConfigsDirectory ?? (configsDirectory == null ? LauncherPaths.DirLegacyKeybindConfigs : null);
        _legacyConfigPath = legacyConfigPath ?? (configPath == null ? LauncherPaths.PathLegacyKeybindConfigsConfig : null);
        _pathComparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    }

    public string ConfigsDirectory => _configsDirectory;
    public string ClientKeybindsPath => _clientKeybindsPath;

    public void Initialize()
    {
        Helpers.EnsureDirectoryExists(_configsDirectory);
        MigrateLegacyConfigFiles();
        MigrateLegacySelection();
    }

    public IReadOnlyList<KeybindConfigInfo> LoadConfigs()
    {
        Initialize();

        var selectedFileName = LoadSelectedFileName();

        return EnumerateConfigFiles()
            .Select(path => new KeybindConfigInfo(
                path,
                selectedFileName != null && _pathComparer.Equals(Path.GetFileName(path), selectedFileName)))
            .ToArray();
    }

    public void ClearSelection()
    {
        SaveSelection(null);
    }

    public void SelectConfig(KeybindConfigInfo config)
    {
        SelectConfig(config.FilePath);
    }

    public void SelectConfig(string filePath)
    {
        Initialize();

        var fullPath = Path.GetFullPath(filePath);
        if (!IsConfigFile(fullPath))
            throw new InvalidOperationException($"Not a keybind config file: {filePath}");

        SaveSelection(Path.GetFileName(fullPath));
    }

    public void DeleteConfig(KeybindConfigInfo config)
    {
        DeleteConfig(config.FilePath);
    }

    public void DeleteConfig(string filePath)
    {
        Initialize();

        var fullPath = Path.GetFullPath(filePath);
        if (!IsConfigFile(fullPath))
            throw new InvalidOperationException($"Not a keybind config file: {filePath}");

        var selectedFileName = LoadSelectedFileName();
        var fileName = Path.GetFileName(fullPath);

        DeleteFileIfExists(fullPath);

        if (!string.IsNullOrWhiteSpace(_legacyConfigsDirectory))
            DeleteFileIfExists(Path.Combine(_legacyConfigsDirectory, fileName));

        if (selectedFileName != null && _pathComparer.Equals(selectedFileName, fileName))
            SaveSelection(null);
    }

    public string? ApplySelectedConfig()
    {
        Initialize();

        var selectedFileName = LoadSelectedFileName();
        if (string.IsNullOrWhiteSpace(selectedFileName))
            return null;

        var selectedPath = Path.GetFullPath(Path.Combine(_configsDirectory, selectedFileName));
        if (!IsConfigFile(selectedPath) || !File.Exists(selectedPath))
        {
            Log.Warning("Selected keybind config is missing or invalid: {ConfigPath}", selectedPath);
            return null;
        }

        Helpers.EnsureDirectoryExists(Path.GetDirectoryName(_clientKeybindsPath)!);
        File.Copy(selectedPath, _clientKeybindsPath, true);
        Log.Information(
            "Applied keybind config {ConfigFile} to {ClientKeybindsPath}",
            selectedFileName,
            _clientKeybindsPath);

        return selectedPath;
    }

    public KeybindConfigInfo? ImportCurrentKeybinds()
    {
        Initialize();

        if (!File.Exists(_clientKeybindsPath))
            return null;

        var importedPath = GetUniqueConfigPath("current-keybinds.yml");
        File.Copy(_clientKeybindsPath, importedPath);

        SelectConfig(importedPath);
        return new KeybindConfigInfo(importedPath, true);
    }

    private IEnumerable<string> EnumerateConfigFiles()
    {
        return EnumerateConfigFiles(_configsDirectory);
    }

    private void MigrateLegacyConfigFiles()
    {
        if (string.IsNullOrWhiteSpace(_legacyConfigsDirectory) ||
            _pathComparer.Equals(Path.GetFullPath(_legacyConfigsDirectory), _configsDirectory) ||
            !Directory.Exists(_legacyConfigsDirectory))
        {
            return;
        }

        foreach (var legacyPath in EnumerateConfigFiles(_legacyConfigsDirectory))
        {
            var targetPath = Path.Combine(_configsDirectory, Path.GetFileName(legacyPath));
            if (File.Exists(targetPath))
                continue;

            try
            {
                File.Copy(legacyPath, targetPath);
                Log.Information("Migrated keybind config {ConfigFile} to data configs directory", Path.GetFileName(legacyPath));
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Failed to migrate keybind config {ConfigPath}", legacyPath);
            }
        }
    }

    private void MigrateLegacySelection()
    {
        if (File.Exists(_configPath) ||
            string.IsNullOrWhiteSpace(_legacyConfigPath) ||
            _pathComparer.Equals(Path.GetFullPath(_legacyConfigPath), _configPath) ||
            !File.Exists(_legacyConfigPath))
        {
            return;
        }

        try
        {
            File.Copy(_legacyConfigPath, _configPath);
        }
        catch (Exception exception)
        {
            Log.Warning(exception, "Failed to migrate keybind config selection from {ConfigPath}", _legacyConfigPath);
        }
    }

    private IEnumerable<string> EnumerateConfigFiles(string directory)
    {
        return Directory.EnumerateFiles(directory, "*.yml")
            .Concat(Directory.EnumerateFiles(directory, "*.yaml"))
            .Distinct(_pathComparer)
            .OrderBy(Path.GetFileName, _pathComparer);
    }

    private bool IsConfigFile(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        if (!IsPathInsideDirectory(fullPath, _configsDirectory))
            return false;

        var extension = Path.GetExtension(fullPath);
        return string.Equals(extension, ".yml", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(extension, ".yaml", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsPathInsideDirectory(string filePath, string directoryPath)
    {
        var relativePath = Path.GetRelativePath(directoryPath, filePath);
        return !relativePath.StartsWith("..", StringComparison.Ordinal) &&
               !Path.IsPathRooted(relativePath);
    }

    private static void DeleteFileIfExists(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        File.SetAttributes(filePath, FileAttributes.Normal);
        File.Delete(filePath);
    }

    private string GetUniqueConfigPath(string fileName)
    {
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var candidate = Path.Combine(_configsDirectory, fileName);

        for (var i = 2; File.Exists(candidate); i++)
        {
            candidate = Path.Combine(_configsDirectory, $"{baseName}-{i}{extension}");
        }

        return candidate;
    }

    private string? LoadSelectedFileName()
    {
        if (!File.Exists(_configPath))
            return null;

        try
        {
            var json = File.ReadAllText(_configPath, Encoding.UTF8);
            var config = JsonSerializer.Deserialize<KeybindConfigSelection>(json, ConfigJsonOptions);
            return string.IsNullOrWhiteSpace(config?.FileName)
                ? null
                : Path.GetFileName(config.FileName);
        }
        catch (Exception exception)
        {
            Log.Warning(exception, "Failed to load keybind config selection from {ConfigPath}", _configPath);
            return null;
        }
    }

    private void SaveSelection(string? fileName)
    {
        Initialize();

        var config = new KeybindConfigSelection(fileName);
        var json = JsonSerializer.Serialize(config, ConfigJsonOptions);
        File.WriteAllText(_configPath, json, Encoding.UTF8);
    }

    private sealed record KeybindConfigSelection(string? FileName);
}
