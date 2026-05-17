using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ReactiveUI;
using Serilog;
using Splat;
using SS14.Launcher.Localization;
using SS14.Launcher.Models.KeybindConfigs;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public sealed class ConfigsTabViewModel : MainWindowTabViewModel
{
    private readonly LocalizationManager _loc;
    private readonly KeybindConfigManager _keybindConfigManager;

    public ConfigsTabViewModel()
    {
        _loc = LocalizationManager.Instance;
        _keybindConfigManager = Locator.Current.GetService<KeybindConfigManager>()!;
        Configs.CollectionChanged += ConfigsOnCollectionChanged;
        ReloadConfigs();
    }

    public override string Name => _loc.GetString("tab-configs-title");

    public ObservableCollection<KeybindConfigInfo> Configs { get; } = new();

    public string ConfigsDirectory => _keybindConfigManager.ConfigsDirectory;

    public string ClientKeybindsPath => _keybindConfigManager.ClientKeybindsPath;

    public bool HasConfigs => Configs.Count != 0;

    public bool HasSelectedConfig => SelectedConfig != null;

    public bool CanImportCurrentKeybinds => File.Exists(ClientKeybindsPath);

    public KeybindConfigInfo? SelectedConfig => Configs.FirstOrDefault(config => config.Selected);

    public string ActiveConfigName => SelectedConfig?.Name ?? _loc.GetString("tab-configs-active-default");

    public override void Selected()
    {
        ReloadConfigs();
    }

    public void ReloadConfigs()
    {
        var configs = _keybindConfigManager.LoadConfigs();

        Configs.Clear();
        foreach (var config in configs)
        {
            Configs.Add(config);
        }

        RaiseStateChanged();
    }

    public void OpenConfigsDirectory()
    {
        Directory.CreateDirectory(ConfigsDirectory);
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = ConfigsDirectory
        });
    }

    public void SelectConfig(KeybindConfigInfo config)
    {
        _keybindConfigManager.SelectConfig(config);
        Log.Debug("Selected keybind config {ConfigFile}", config.FileName);
        ReloadConfigs();
    }

    public void ClearSelectedConfig()
    {
        _keybindConfigManager.ClearSelection();
        Log.Debug("Cleared selected keybind config");
        ReloadConfigs();
    }

    public void ImportCurrentKeybinds()
    {
        var imported = _keybindConfigManager.ImportCurrentKeybinds();
        if (imported == null)
            return;

        Log.Debug("Imported current keybinds into {ConfigFile}", imported.FileName);
        ReloadConfigs();
    }

    public void DeleteConfig(KeybindConfigInfo config)
    {
        _keybindConfigManager.DeleteConfig(config);
        Log.Debug("Deleted keybind config {ConfigFile}", config.FileName);
        ReloadConfigs();
    }

    private void ConfigsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        RaiseStateChanged();
    }

    private void RaiseStateChanged()
    {
        this.RaisePropertyChanged(nameof(HasConfigs));
        this.RaisePropertyChanged(nameof(HasSelectedConfig));
        this.RaisePropertyChanged(nameof(CanImportCurrentKeybinds));
        this.RaisePropertyChanged(nameof(SelectedConfig));
        this.RaisePropertyChanged(nameof(ActiveConfigName));
        this.RaisePropertyChanged(nameof(ConfigsDirectory));
        this.RaisePropertyChanged(nameof(ClientKeybindsPath));
    }
}
