using Avalonia.Controls;
using Avalonia.Interactivity;
using SS14.Launcher.Models.ResourcePacks;
using SS14.Launcher.ViewModels.MainWindowTabs;

namespace SS14.Launcher.Views.MainWindowTabs;

public partial class ResourcePacksTabView : UserControl
{
    public ResourcePacksTabView()
    {
        InitializeComponent();
    }

    private ResourcePacksTabViewModel ViewModel => (ResourcePacksTabViewModel)DataContext!;

    public void OpenResourcePackDirectoryPressed(object? sender, RoutedEventArgs args)
    {
        ViewModel.OpenResourcePackDirectory();
    }

    public void ReloadResourcePacksPressed(object? sender, RoutedEventArgs args)
    {
        ViewModel.ReloadPacks();
    }

    public void ResourcePackEnabledChanged(object? sender, RoutedEventArgs args)
    {
        if (sender is not CheckBox checkBox || checkBox.DataContext is not ResourcePackInfo pack)
            return;

        ViewModel.SetResourcePackEnabled(pack, checkBox.IsChecked == true);
    }

    public void MoveResourcePackUpPressed(object? sender, RoutedEventArgs args)
    {
        if (sender is not Button button || button.DataContext is not ResourcePackInfo pack)
            return;

        ViewModel.MoveResourcePack(pack, -1);
    }

    public void MoveResourcePackDownPressed(object? sender, RoutedEventArgs args)
    {
        if (sender is not Button button || button.DataContext is not ResourcePackInfo pack)
            return;

        ViewModel.MoveResourcePack(pack, 1);
    }
}
