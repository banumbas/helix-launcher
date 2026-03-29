using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Splat;
using SS14.Launcher.Localization;
using SS14.Launcher.Utility;
using SS14.Launcher.ViewModels.MainWindowTabs;
using SS14.Launcher.Views;

namespace SS14.Launcher.Views.Helix;

public sealed partial class OptionsTabHelixView : UserControl
{
    public OptionsTabHelixView()
    {
        InitializeComponent();

        SetHelixSectionVisible(false);
    }

    private void ShowGeneralClicked(object? sender, RoutedEventArgs args)
    {
        SetHelixSectionVisible(false);
    }

    private void ShowHelixClicked(object? sender, RoutedEventArgs args)
    {
        SetHelixSectionVisible(true);
    }

    private void SetHelixSectionVisible(bool showHelix)
    {
        GeneralPanel.IsVisible = !showHelix;
        HelixPanel.IsVisible = showHelix;
        GeneralTabButton.Classes.Set("selected", !showHelix);
        HelixLauncherTabButton.Classes.Set("selected", showHelix);
    }

    public async void ClearEnginesPressed(object? _1, RoutedEventArgs _2)
    {
        ((OptionsTabViewModel)DataContext!).ClearEngines();
        await ClearEnginesButton.DisplayDoneMessage();
    }

    public async void ClearServerContentPressed(object? _1, RoutedEventArgs _2)
    {
        var blocked = !await ((OptionsTabViewModel)DataContext!).ClearServerContent();
        var locMgr = Locator.Current.GetService<LocalizationManager>()!;

        await ClearServerContentButton.DisplayDoneMessage(
            blocked ? locMgr.GetString("tab-options-clear-content-close-client") : null);
    }

    private async void OpenHubSettings(object? sender, RoutedEventArgs args)
    {
        await new HubSettingsDialog().ShowDialog((Window)this.GetVisualRoot()!);
    }
}
