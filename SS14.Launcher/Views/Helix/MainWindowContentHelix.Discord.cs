using System;
using System.Linq;
using Microsoft.Toolkit.Mvvm.Messaging;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.Helix;

namespace SS14.Launcher.Views.Helix;

public sealed partial class MainWindowContentHelix
{
    private bool _gameRunning = HelixGameActivity.IsGameRunning;

    private void InitializeDiscordPresence()
    {
        WeakReferenceMessenger.Default.Register<HelixGameStartedMessage>(this, (_, _) =>
        {
            _gameRunning = true;
            UpdateDiscordPresence();
        });

        WeakReferenceMessenger.Default.Register<HelixGameExitedMessage>(this, (_, _) =>
        {
            _gameRunning = false;
            UpdateDiscordPresence();
        });

        WeakReferenceMessenger.Default.Register<HelixDiscordRichPresenceSettingsChanged>(
            this,
            (_, _) => UpdateDiscordPresence());

        HelixDiscordRichPresence.Instance.SetActivity("Starting launcher");
    }

    private void UpdateDiscordPresence()
    {
        if (_viewModel == null)
        {
            HelixDiscordRichPresence.Instance.SetActivity("Starting launcher");
            return;
        }

        if (!_viewModel.Cfg.GetCVar(CVars.HelixDiscordRichPresenceEnabled))
        {
            HelixDiscordRichPresence.Instance.Stop();
            return;
        }

        if (_viewModel.ConnectingVM != null)
        {
            HelixDiscordRichPresence.Instance.SetActivity("Launching a server");
            return;
        }

        if (_gameRunning)
        {
            var mode = HelixDiscordPresenceSettings.GetInGamePresenceMode(
                _viewModel.Cfg.GetCVar(CVars.HelixDiscordInGamePresenceMode));
            if (mode == HelixDiscordInGamePresenceMode.Helix)
            {
                HelixDiscordRichPresence.Instance.SetActivity(BuildGameActivity(HelixGameActivity.Presence));
                return;
            }

            HelixDiscordRichPresence.Instance.Stop();
            return;
        }

        if (!_viewModel.LoggedIn)
        {
            HelixDiscordRichPresence.Instance.SetActivity(
                string.IsNullOrWhiteSpace(_viewModel.BusyTask)
                    ? "At login screen"
                    : _viewModel.BusyTask);
            return;
        }

        if (_viewModel.Tabs.Count == 0)
        {
            HelixDiscordRichPresence.Instance.SetActivity("In launcher");
            return;
        }

        var selectedIndex = Math.Clamp(_viewModel.SelectedIndex, 0, _viewModel.Tabs.Count - 1);
        var state = selectedIndex switch
        {
            0 => "Viewing home",
            1 => "Browsing servers",
            2 => "Reading news",
            3 => "Managing resource packs",
            4 => "Managing keybind configs",
            5 => "Changing settings",
            _ => $"Viewing {_viewModel.Tabs[selectedIndex].Name}"
        };

        HelixDiscordRichPresence.Instance.SetActivity(state);
    }

    private static HelixDiscordActivity BuildGameActivity(HelixGamePresence? presence)
    {
        if (presence == null)
        {
            return new HelixDiscordActivity(
                Details: "Playing via Helix Launcher",
                State: "Space Station 14");
        }

        var server = FirstNonBlank(presence.ServerName, presence.ServerAddress, "Space Station 14");
        var details = $"Playing via Helix on {server}";

        var stateParts = new[]
        {
            presence.Username,
            FirstNonBlank(presence.Preset, presence.Map)
        }.Where(x => !string.IsNullOrWhiteSpace(x));

        var state = string.Join(" - ", stateParts);
        if (string.IsNullOrWhiteSpace(state))
            state = "Space Station 14";

        return new HelixDiscordActivity(
            Details: details,
            State: state,
            PlayerCount: presence.PlayerCount,
            MaxPlayers: presence.SoftMaxPlayerCount);
    }

    private static string? FirstNonBlank(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }
}
