using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Splat;
using SS14.Launcher.Localization;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.ResourcePacks;
using SS14.Launcher.ViewModels.MainWindowTabs;

namespace SS14.Launcher.Tests;

[TestFixture]
public sealed class ResourcePackManagerTests
{
    [Test]
    public void LoadPacks_PreservesSavedOrderAndEnabledFlags()
    {
        using var fixture = new ResourcePackFixture();

        fixture.CreatePack("01-alpha", "Alpha", "");
        fixture.CreatePack("02-beta", "Beta", "");

        var manager = fixture.CreateManager();
        var packs = manager.LoadPacks().ToArray();

        packs[0].Enabled = false;
        manager.SavePacks([packs[1], packs[0]]);

        var reloaded = manager.LoadPacks().ToArray();

        Assert.That(reloaded.Select(pack => pack.Name), Is.EqualTo(new[] { "Beta", "Alpha" }));
        Assert.That(reloaded[0].Enabled, Is.True);
        Assert.That(reloaded[1].Enabled, Is.False);
    }

    [Test]
    public async Task BuildOverlayZipAsync_PrioritizesHigherPacksAndHonorsTargetForkId()
    {
        using var fixture = new ResourcePackFixture();

        fixture.CreatePack(
            "01-top",
            "Top",
            "",
            ("Locale/en-US/test.ftl", "top"),
            ("Textures/shared.png", "top-texture"));

        fixture.CreatePack(
            "02-targeted",
            "Targeted",
            "fork-b",
            ("Locale/en-US/test.ftl", "targeted"),
            ("Textures/targeted.png", "targeted-texture"));

        var manager = fixture.CreateManager();
        var packs = manager.LoadPacks();

        var overlayWithoutTarget = await manager.BuildOverlayZipAsync(packs, null);
        Assert.That(overlayWithoutTarget, Is.Not.Null);

        using (var archive = ZipFile.OpenRead(overlayWithoutTarget!))
        {
            Assert.That(ReadEntryText(archive, "Locale/en-US/test.ftl"), Is.EqualTo("top"));
            Assert.That(archive.GetEntry("Textures/shared.png"), Is.Not.Null);
            Assert.That(archive.GetEntry("Textures/targeted.png"), Is.Null);
        }

        var overlayWithTarget = await manager.BuildOverlayZipAsync(packs, "fork-b");
        Assert.That(overlayWithTarget, Is.Not.Null);

        using var targetedArchive = ZipFile.OpenRead(overlayWithTarget!);
        Assert.That(ReadEntryText(targetedArchive, "Locale/en-US/test.ftl"), Is.EqualTo("top"));
        Assert.That(ReadEntryText(targetedArchive, "Textures/targeted.png"), Is.EqualTo("targeted-texture"));
    }

    [Test]
    [NonParallelizable]
    public void SetResourcePackEnabled_SavesStateEvenWhenBindingAlreadyUpdatedTheModel()
    {
        using var fixture = new ResourcePackFixture();

        fixture.CreatePack("01-alpha", "Alpha", "");

        var manager = fixture.CreateManager();
        Locator.CurrentMutable.RegisterConstant(manager);
        Locator.CurrentMutable.RegisterConstant(new LocalizationManager(new DataManager()));

        var viewModel = new ResourcePacksTabViewModel();
        var pack = viewModel.ResourcePacks.Single();

        // Simulate Avalonia's two-way binding updating the model before the click handler runs.
        pack.Enabled = false;

        viewModel.SetResourcePackEnabled(pack, false);

        var reloaded = manager.LoadPacks().Single();
        Assert.That(reloaded.Enabled, Is.False);
    }

    private static string ReadEntryText(ZipArchive archive, string entryPath)
    {
        var entry = archive.GetEntry(entryPath);
        Assert.That(entry, Is.Not.Null, $"Missing archive entry: {entryPath}");

        using var stream = entry!.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private sealed class ResourcePackFixture : IDisposable
    {
        private readonly string _rootDirectory;
        private readonly string _packsDirectory;
        private readonly string _configPath;
        private readonly string _overlayCacheDirectory;

        public ResourcePackFixture()
        {
            _rootDirectory = Path.Combine(Path.GetTempPath(), "ss14-resource-pack-tests", Guid.NewGuid().ToString("N"));
            _packsDirectory = Path.Combine(_rootDirectory, "packs");
            _configPath = Path.Combine(_rootDirectory, "resource_packs.json");
            _overlayCacheDirectory = Path.Combine(_rootDirectory, "cache");

            Directory.CreateDirectory(_packsDirectory);
            Directory.CreateDirectory(_overlayCacheDirectory);
        }

        public ResourcePackManager CreateManager()
        {
            return new ResourcePackManager(_packsDirectory, _configPath, _overlayCacheDirectory);
        }

        public void CreatePack(string folderName, string name, string target, params (string RelativePath, string Contents)[] files)
        {
            var packDirectory = Path.Combine(_packsDirectory, folderName);
            Directory.CreateDirectory(packDirectory);

            File.WriteAllText(
                Path.Combine(packDirectory, "meta.json"),
                $$"""
                {
                  "name": "{{name}}",
                  "description": "test pack",
                  "target": "{{target}}"
                }
                """);

            foreach (var (relativePath, contents) in files)
            {
                var filePath = Path.Combine(packDirectory, "Resources", relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                File.WriteAllText(filePath, contents);
            }
        }

        public void Dispose()
        {
            if (Directory.Exists(_rootDirectory))
                Directory.Delete(_rootDirectory, true);
        }
    }
}
