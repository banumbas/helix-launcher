using Avalonia.Controls;
using Avalonia.Interactivity;
using SS14.Launcher.Models.KeybindConfigs;
using SS14.Launcher.ViewModels.MainWindowTabs;

namespace SS14.Launcher.Views.MainWindowTabs;

public partial class ConfigsTabView : UserControl
{
    public ConfigsTabView()
    {
        InitializeComponent();
    }

    private ConfigsTabViewModel ViewModel => (ConfigsTabViewModel)DataContext!;

    public void OpenConfigsDirectoryPressed(object? sender, RoutedEventArgs args)
    {
        ViewModel.OpenConfigsDirectory();
    }

    public void ReloadConfigsPressed(object? sender, RoutedEventArgs args)
    {
        ViewModel.ReloadConfigs();
    }

    public void ImportCurrentKeybindsPressed(object? sender, RoutedEventArgs args)
    {
        ViewModel.ImportCurrentKeybinds();
    }

    public void ClearSelectedConfigPressed(object? sender, RoutedEventArgs args)
    {
        ViewModel.ClearSelectedConfig();
    }

    public void SelectConfigPressed(object? sender, RoutedEventArgs args)
    {
        if (sender is not Button button || button.DataContext is not KeybindConfigInfo config)
            return;

        ViewModel.SelectConfig(config);
    }

    public void DeleteConfigPressed(object? sender, RoutedEventArgs args)
    {
        if (sender is not Button button || button.DataContext is not KeybindConfigInfo config)
            return;

        ViewModel.DeleteConfig(config);
    }
}
