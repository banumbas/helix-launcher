using Avalonia.Controls;
using Avalonia.Interactivity;
using SS14.Launcher.ViewModels.MainWindowTabs;

namespace SS14.Launcher.Views.Helix;

public sealed partial class ServerListTabHelixView : UserControl
{
    private ServerListTabViewModel? ViewModel => DataContext as ServerListTabViewModel;

    public ServerListTabHelixView()
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
