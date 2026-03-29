using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Splat;
using SS14.Launcher.Localization;
using SS14.Launcher.Utility;
using SS14.Launcher.ViewModels.MainWindowTabs;
using SS14.Launcher.Views;

namespace SS14.Launcher.Views.Worm;

public sealed partial class OptionsTabWormView : UserControl
{
    public OptionsTabWormView()
    {
        InitializeComponent();

        SetWormSectionVisible(false);
    }

    private void ShowGeneralClicked(object? sender, RoutedEventArgs args)
    {
        SetWormSectionVisible(false);
    }

    private void ShowWormClicked(object? sender, RoutedEventArgs args)
    {
        SetWormSectionVisible(true);
    }

    private void SetWormSectionVisible(bool showWorm)
    {
        GeneralPanel.IsVisible = !showWorm;
        WormPanel.IsVisible = showWorm;
        GeneralTabButton.Classes.Set("selected", !showWorm);
        WormLauncherTabButton.Classes.Set("selected", showWorm);
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
