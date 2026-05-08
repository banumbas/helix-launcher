using Microsoft.Toolkit.Mvvm.Messaging;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.Helix;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public partial class OptionsTabViewModel
{
    public bool HelixDiscordRichPresenceEnabled
    {
        get => Cfg.GetCVar(CVars.HelixDiscordRichPresenceEnabled);
        set
        {
            Cfg.SetCVar(CVars.HelixDiscordRichPresenceEnabled, value);
            Cfg.CommitConfig();
            NotifyHelixDiscordRichPresenceSettingsChanged();
        }
    }

    public bool HelixDiscordInGameUseOriginal
    {
        get => GetHelixDiscordInGamePresenceMode() == HelixDiscordInGamePresenceMode.Original;
        set
        {
            if (value)
                SetHelixDiscordInGamePresenceMode(HelixDiscordInGamePresenceMode.Original);
        }
    }

    public bool HelixDiscordInGameUseHelix
    {
        get => GetHelixDiscordInGamePresenceMode() == HelixDiscordInGamePresenceMode.Helix;
        set
        {
            if (value)
                SetHelixDiscordInGamePresenceMode(HelixDiscordInGamePresenceMode.Helix);
        }
    }

    private HelixDiscordInGamePresenceMode GetHelixDiscordInGamePresenceMode()
    {
        var value = Cfg.GetCVar(CVars.HelixDiscordInGamePresenceMode);
        return HelixDiscordPresenceSettings.GetInGamePresenceMode(value);
    }

    private void SetHelixDiscordInGamePresenceMode(HelixDiscordInGamePresenceMode mode)
    {
        Cfg.SetCVar(CVars.HelixDiscordInGamePresenceMode, (int)mode);
        Cfg.CommitConfig();
        NotifyHelixDiscordRichPresenceSettingsChanged();
    }

    private static void NotifyHelixDiscordRichPresenceSettingsChanged()
    {
        WeakReferenceMessenger.Default.Send(new HelixDiscordRichPresenceSettingsChanged());
    }
}
