using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Messaging;
using Splat;
using SS14.Launcher.Localization;
using SS14.Launcher.Models.ContentManagement;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.EngineManager;
using SS14.Launcher.Utility;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public class OptionsTabViewModel : MainWindowTabViewModel
{
    public DataManager Cfg { get; }
    private readonly IEngineManager _engineManager;
    private readonly ContentManager _contentManager;

    public LanguageSelectorViewModel Language { get; } = new();

    public OptionsTabViewModel()
    {
        Cfg = Locator.Current.GetRequiredService<DataManager>();
        _engineManager = Locator.Current.GetRequiredService<IEngineManager>();
        _contentManager = Locator.Current.GetRequiredService<ContentManager>();

        DisableIncompatibleMacOS = OperatingSystem.IsMacOS();
    }
    public bool DisableIncompatibleMacOS { get; }

    public override string Name => LocalizationManager.Instance.GetString("tab-options-title");

    public bool CompatMode
    {
        get => Cfg.GetCVar(CVars.CompatMode);
        set
        {
            Cfg.SetCVar(CVars.CompatMode, value);
            Cfg.CommitConfig();
        }
    }

    public bool LogLauncherVerbose
    {
        get => Cfg.GetCVar(CVars.LogLauncherVerbose);
        set
        {
            Cfg.SetCVar(CVars.LogLauncherVerbose, value);
            Cfg.CommitConfig();
        }
    }

    public bool OverrideAssets
    {
        get => Cfg.GetCVar(CVars.OverrideAssets);
        set
        {
            Cfg.SetCVar(CVars.OverrideAssets, value);
            Cfg.CommitConfig();
        }
    }

    // Worm-Start
    public bool ServerListShowMap
    {
        get => Cfg.GetCVar(CVars.ServerListShowMap);
        set
        {
            Cfg.SetCVar(CVars.ServerListShowMap, value);
            Cfg.CommitConfig();
            NotifyServerListDisplaySettingsChanged();
        }
    }

    public bool ServerListShowMode
    {
        get => Cfg.GetCVar(CVars.ServerListShowMode);
        set
        {
            Cfg.SetCVar(CVars.ServerListShowMode, value);
            Cfg.CommitConfig();
            NotifyServerListDisplaySettingsChanged();
        }
    }

    public bool ServerListShowPing
    {
        get => Cfg.GetCVar(CVars.ServerListShowPing);
        set
        {
            Cfg.SetCVar(CVars.ServerListShowPing, value);
            Cfg.CommitConfig();
            NotifyServerListDisplaySettingsChanged();
        }
    }
    // Worm-End

    public void ClearEngines()
    {
        _engineManager.ClearAllEngines();
    }

    public async Task<bool> ClearServerContent()
    {
        return await _contentManager.ClearAll();
    }

    public void OpenLogDirectory()
    {
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = LauncherPaths.DirLogs
        });
    }

    public void OpenAccountSettings()
    {
        Helpers.OpenUri(ConfigConstants.AccountManagementUrl);
    }

    // Worm-Start
    private static void NotifyServerListDisplaySettingsChanged()
    {
        WeakReferenceMessenger.Default.Send(new ServerListDisplaySettingsChanged());
    }
    // Worm-End
}
