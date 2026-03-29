using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using SS14.Launcher.ViewModels.MainWindowTabs;

namespace SS14.Launcher.Views.Helix;

public sealed partial class ServerListHelix : UserControl
{
    public static readonly StyledProperty<bool> ShowMapColumnProperty =
        AvaloniaProperty.Register<ServerListHelix, bool>(nameof(ShowMapColumn), true);

    public static readonly StyledProperty<bool> ShowModeColumnProperty =
        AvaloniaProperty.Register<ServerListHelix, bool>(nameof(ShowModeColumn), true);

    public static readonly StyledProperty<bool> ShowPingColumnProperty =
        AvaloniaProperty.Register<ServerListHelix, bool>(nameof(ShowPingColumn), true);

    public static readonly StyledProperty<bool> ShowHeaderProperty =
        AvaloniaProperty.Register<ServerListHelix, bool>(nameof(ShowHeader));

    public static readonly StyledProperty<bool> SpinnerVisibleProperty =
        AvaloniaProperty.Register<ServerListHelix, bool>(nameof(SpinnerVisible));

    public static readonly StyledProperty<string?> ListTextProperty =
        AvaloniaProperty.Register<ServerListHelix, string?>(nameof(ListText));

    public static readonly StyledProperty<IReadOnlyCollection<ServerEntryViewModel>> ListProperty =
        AvaloniaProperty.Register<ServerListHelix, IReadOnlyCollection<ServerEntryViewModel>>(
            nameof(List),
            Array.Empty<ServerEntryViewModel>());

    public static readonly StyledProperty<bool> UseInternalScrollViewerProperty =
        AvaloniaProperty.Register<ServerListHelix, bool>(nameof(UseInternalScrollViewer), true);

    public bool ShowMapColumn
    {
        get => GetValue(ShowMapColumnProperty);
        set => SetValue(ShowMapColumnProperty, value);
    }

    public bool ShowModeColumn
    {
        get => GetValue(ShowModeColumnProperty);
        set => SetValue(ShowModeColumnProperty, value);
    }

    public bool ShowPingColumn
    {
        get => GetValue(ShowPingColumnProperty);
        set => SetValue(ShowPingColumnProperty, value);
    }

    public bool ShowHeader
    {
        get => GetValue(ShowHeaderProperty);
        set => SetValue(ShowHeaderProperty, value);
    }

    public bool SpinnerVisible
    {
        get => GetValue(SpinnerVisibleProperty);
        set => SetValue(SpinnerVisibleProperty, value);
    }

    public string? ListText
    {
        get => GetValue(ListTextProperty);
        set => SetValue(ListTextProperty, value);
    }

    public IReadOnlyCollection<ServerEntryViewModel> List
    {
        get => GetValue(ListProperty);
        set => SetValue(ListProperty, value);
    }

    public bool UseInternalScrollViewer
    {
        get => GetValue(UseInternalScrollViewerProperty);
        set => SetValue(UseInternalScrollViewerProperty, value);
    }

    public ServerListHelix()
    {
        InitializeComponent();
    }
}
