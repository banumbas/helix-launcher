using Avalonia.Controls;
using Avalonia.Interactivity;
using SS14.Launcher.ViewModels.MainWindowTabs;

namespace SS14.Launcher.Views.Worm;

public sealed partial class ServerListTabWormView : UserControl
{
    private ServerListTabViewModel? ViewModel => DataContext as ServerListTabViewModel;

    public ServerListTabWormView()
    {
        InitializeComponent();
    }

    private void RefreshClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel?.RefreshPressed();
    }

    private void DirectConnectClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel?.DirectConnectPressed();
    }
}
