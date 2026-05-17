using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SS14.Launcher.Models.KeybindConfigs;

namespace SS14.Launcher.Tests;

[TestFixture]
public sealed class KeybindConfigManagerTests
{
    [Test]
    public void LoadConfigs_FindsYamlFilesAndPreservesSelection()
    {
        using var fixture = new KeybindConfigFixture();

        fixture.CreateConfig("alpha.yml", "binds: []");
        fixture.CreateConfig("beta.yaml", "binds: []");
        fixture.CreateConfig("notes.txt", "not a config");

        var manager = fixture.CreateManager();
        var configs = manager.LoadConfigs().ToArray();

        Assert.That(configs.Select(config => config.FileName), Is.EqualTo(new[] { "alpha.yml", "beta.yaml" }));

        manager.SelectConfig(configs[1]);
        var reloaded = manager.LoadConfigs().ToArray();

        Assert.That(reloaded.Single(config => config.Selected).FileName, Is.EqualTo("beta.yaml"));
    }

    [Test]
    public void ApplySelectedConfig_CopiesSelectedProfileToClientKeybinds()
    {
        using var fixture = new KeybindConfigFixture();

        var configPath = fixture.CreateConfig("alpha.yml", "binds:\n- type: test\n");
        var manager = fixture.CreateManager();

        manager.SelectConfig(configPath);
        var appliedPath = manager.ApplySelectedConfig();

        Assert.That(appliedPath, Is.EqualTo(Path.GetFullPath(configPath)));
        Assert.That(File.ReadAllText(fixture.ClientKeybindsPath), Is.EqualTo("binds:\n- type: test\n"));
    }

    [Test]
    public void ImportCurrentKeybinds_CreatesSelectedConfigInData()
    {
        using var fixture = new KeybindConfigFixture();

        Directory.CreateDirectory(Path.GetDirectoryName(fixture.ClientKeybindsPath)!);
        File.WriteAllText(fixture.ClientKeybindsPath, "binds:\n- type: imported\n");

        var manager = fixture.CreateManager();
        var imported = manager.ImportCurrentKeybinds();

        Assert.That(imported, Is.Not.Null);
        Assert.That(imported!.FileName, Is.EqualTo("current-keybinds.yml"));
        Assert.That(Path.GetDirectoryName(imported.FilePath), Is.EqualTo(fixture.ConfigsDirectory));
        Assert.That(File.ReadAllText(imported.FilePath), Is.EqualTo("binds:\n- type: imported\n"));

        var selected = manager.LoadConfigs().Single(config => config.Selected);
        Assert.That(selected.FileName, Is.EqualTo("current-keybinds.yml"));
    }

    [Test]
    public void LoadConfigs_MigratesLegacyLauncherConfigsIntoData()
    {
        using var fixture = new KeybindConfigFixture();

        fixture.CreateLegacyConfig("legacy.yml", "binds:\n- type: legacy\n");
        File.WriteAllText(fixture.LegacyConfigPath, """{"FileName":"legacy.yml"}""");

        var manager = fixture.CreateManager();
        var configs = manager.LoadConfigs().ToArray();

        Assert.That(File.Exists(Path.Combine(fixture.ConfigsDirectory, "legacy.yml")), Is.True);
        Assert.That(File.Exists(fixture.ConfigPath), Is.True);
        Assert.That(configs.Single(config => config.Selected).FileName, Is.EqualTo("legacy.yml"));
    }

    [Test]
    public void DeleteConfig_RemovesFileAndClearsSelection()
    {
        using var fixture = new KeybindConfigFixture();

        var configPath = fixture.CreateConfig("alpha.yml", "binds: []");
        var legacyPath = fixture.CreateLegacyConfig("alpha.yml", "binds: []");
        var manager = fixture.CreateManager();

        manager.SelectConfig(configPath);
        manager.DeleteConfig(configPath);

        Assert.That(File.Exists(configPath), Is.False);
        Assert.That(File.Exists(legacyPath), Is.False);
        Assert.That(manager.LoadConfigs().Any(config => config.Selected), Is.False);
        Assert.That(manager.LoadConfigs().Any(config => config.FileName == "alpha.yml"), Is.False);
    }

    [Test]
    public void SelectConfig_RejectsFilesOutsideConfigDirectory()
    {
        using var fixture = new KeybindConfigFixture();

        var outsidePath = Path.Combine(fixture.RootDirectory, "outside.yml");
        File.WriteAllText(outsidePath, "binds: []");

        var manager = fixture.CreateManager();

        Assert.That(() => manager.SelectConfig(outsidePath), Throws.InvalidOperationException);
    }

    private sealed class KeybindConfigFixture : IDisposable
    {
        public KeybindConfigFixture()
        {
            RootDirectory = Path.Combine(Path.GetTempPath(), "ss14-keybind-config-tests", Guid.NewGuid().ToString("N"));
            ConfigsDirectory = Path.Combine(RootDirectory, "data", "configs");
            ConfigPath = Path.Combine(RootDirectory, "data", "keybind_configs.json");
            ClientKeybindsPath = Path.Combine(RootDirectory, "data", "keybinds.yml");
            LegacyConfigsDirectory = Path.Combine(RootDirectory, "launcher", "configs");
            LegacyConfigPath = Path.Combine(RootDirectory, "launcher", "keybind_configs.json");

            Directory.CreateDirectory(ConfigsDirectory);
            Directory.CreateDirectory(LegacyConfigsDirectory);
        }

        public string RootDirectory { get; }
        public string ConfigsDirectory { get; }
        public string ConfigPath { get; }
        public string ClientKeybindsPath { get; }
        public string LegacyConfigsDirectory { get; }
        public string LegacyConfigPath { get; }

        public KeybindConfigManager CreateManager()
        {
            return new KeybindConfigManager(
                ConfigsDirectory,
                ConfigPath,
                ClientKeybindsPath,
                LegacyConfigsDirectory,
                LegacyConfigPath);
        }

        public string CreateConfig(string fileName, string contents)
        {
            var configPath = Path.Combine(ConfigsDirectory, fileName);
            File.WriteAllText(configPath, contents);
            return configPath;
        }

        public string CreateLegacyConfig(string fileName, string contents)
        {
            var configPath = Path.Combine(LegacyConfigsDirectory, fileName);
            File.WriteAllText(configPath, contents);
            return configPath;
        }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
                Directory.Delete(RootDirectory, true);
        }
    }
}
