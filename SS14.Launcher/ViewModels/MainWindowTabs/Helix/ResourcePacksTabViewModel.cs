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

/// <summary>
/// Compatibility view model retained for resource pack services and their tests.
/// Resource packs are configured from OptionsTabViewModel in the launcher UI.
/// </summary>
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

    public override void Selected() => ReloadPacks();

    public void ReloadPacks()
    {
        ResourcePacks.Clear();
        foreach (var pack in _resourcePackManager.LoadPacks())
            ResourcePacks.Add(pack);

        this.RaisePropertyChanged(nameof(HasResourcePacks));
        this.RaisePropertyChanged(nameof(PacksDirectory));
    }

    public void OpenResourcePackDirectory()
    {
        Directory.CreateDirectory(PacksDirectory);
        Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = PacksDirectory });
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

        var index = ResourcePacks.IndexOf(pack);
        var nextIndex = index + delta;
        if (index < 0 || nextIndex < 0 || nextIndex >= ResourcePacks.Count)
            return;

        ResourcePacks.Move(index, nextIndex);
        SavePacks();
    }

    private void SavePacks()
    {
        _resourcePackManager.SavePacks(ResourcePacks);
        Log.Debug("Saved {Count} resource pack entries", ResourcePacks.Count);
    }

    private void ResourcePacksOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args) =>
        this.RaisePropertyChanged(nameof(HasResourcePacks));
}
