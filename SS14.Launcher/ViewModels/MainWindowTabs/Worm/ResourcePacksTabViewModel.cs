using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using ReactiveUI;
using Serilog;
using Splat;
using SS14.Launcher.Localization;
using SS14.Launcher.Models.ResourcePacks;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public sealed class ResourcePacksTabViewModel : MainWindowTabViewModel
{
    private readonly LocalizationManager _loc;
    private readonly ResourcePackManager _resourcePackManager;

    public ResourcePacksTabViewModel()
    {
        _loc = LocalizationManager.Instance;
        _resourcePackManager = Locator.Current.GetService<ResourcePackManager>()!;
        ResourcePacks.CollectionChanged += ResourcePacksOnCollectionChanged;
        ReloadPacks();
    }

    public override string Name => _loc.GetString("tab-resource-packs-title");

    public ObservableCollection<ResourcePackInfo> ResourcePacks { get; } = new();

    public string PacksDirectory => _resourcePackManager.PacksDirectory;

    public bool HasResourcePacks => ResourcePacks.Count != 0;

    public override void Selected()
    {
        ReloadPacks();
    }

    public void ReloadPacks()
    {
        var packs = _resourcePackManager.LoadPacks();

        ResourcePacks.Clear();
        foreach (var pack in packs)
        {
            ResourcePacks.Add(pack);
        }

        this.RaisePropertyChanged(nameof(HasResourcePacks));
        this.RaisePropertyChanged(nameof(PacksDirectory));
    }

    public void OpenResourcePackDirectory()
    {
        Directory.CreateDirectory(PacksDirectory);
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = PacksDirectory
        });
    }

    public void SetResourcePackEnabled(ResourcePackInfo pack, bool enabled)
    {
        pack.Enabled = enabled;
        SavePacks();
    }

    public void MoveResourcePack(ResourcePackInfo? pack, int delta)
    {
        if (pack == null || delta == 0)
            return;

        var currentIndex = ResourcePacks.IndexOf(pack);
        if (currentIndex < 0)
            return;

        var nextIndex = currentIndex + delta;
        if (nextIndex < 0 || nextIndex >= ResourcePacks.Count)
            return;

        ResourcePacks.Move(currentIndex, nextIndex);
        SavePacks();
    }

    private void SavePacks()
    {
        _resourcePackManager.SavePacks(ResourcePacks);
        Log.Debug("Saved {Count} resource pack entries", ResourcePacks.Count);
    }

    private void ResourcePacksOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        this.RaisePropertyChanged(nameof(HasResourcePacks));
    }
}
