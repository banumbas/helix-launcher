using Microsoft.Toolkit.Mvvm.Messaging;
using SS14.Launcher.Models;

namespace SS14.Launcher.Models.Helix;

public enum HelixDiscordInGamePresenceMode
{
    Original = 0,
    Helix = 1
}

public static class HelixDiscordPresenceSettings
{
    public static HelixDiscordInGamePresenceMode GetInGamePresenceMode(int value)
    {
        return value switch
        {
            (int)HelixDiscordInGamePresenceMode.Helix => HelixDiscordInGamePresenceMode.Helix,
            2 => HelixDiscordInGamePresenceMode.Helix,
            _ => HelixDiscordInGamePresenceMode.Original
        };
    }
}

public static class HelixGameActivity
{
    public static bool IsGameRunning { get; private set; }
    public static HelixGamePresence? Presence { get; private set; }

    public static void SetPendingPresence(HelixGamePresence? presence)
    {
        Presence = presence;
    }

    public static void UpdateFromConnectionStatus(Connector.ConnectionStatus status)
    {
        switch (status)
        {
            case Connector.ConnectionStatus.ClientRunning:
                Start();
                break;

            case Connector.ConnectionStatus.ClientExited:
            case Connector.ConnectionStatus.Cancelled:
            case Connector.ConnectionStatus.ConnectionFailed:
            case Connector.ConnectionStatus.UpdateError:
            case Connector.ConnectionStatus.NotAContentBundle:
                Stop();
                break;
        }
    }

    public static void Start()
    {
        if (IsGameRunning)
            return;

        IsGameRunning = true;
        WeakReferenceMessenger.Default.Send(new HelixGameStartedMessage());
    }

    public static void Stop()
    {
        var wasGameRunning = IsGameRunning;

        IsGameRunning = false;
        Presence = null;

        if (!wasGameRunning)
            return;

        WeakReferenceMessenger.Default.Send(new HelixGameExitedMessage());
    }
}

public sealed record HelixGamePresence(
    string? ServerName,
    string? ServerAddress,
    string? Username,
    string? Map,
    string? Preset,
    int? PlayerCount,
    int? SoftMaxPlayerCount);

public sealed record HelixGameStartedMessage;

public sealed record HelixGameExitedMessage;

public sealed record HelixDiscordRichPresenceSettingsChanged;
