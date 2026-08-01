using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Splat;
using SS14.Launcher.Localization;
using SS14.Launcher.Models.KeybindConfigs;
using SS14.Launcher.Models.ResourcePacks;
using SS14.Launcher.Utility;
using SS14.Launcher.ViewModels.MainWindowTabs;

namespace SS14.Launcher.Views.MainWindowTabs;

public partial class OptionsTabView : UserControl
{
    public OptionsTabView()
    {
        InitializeComponent();

        Flip.Command = ReactiveCommand.Create(() =>
        {
            var window = (Window?) VisualRoot;
            if (window == null)
                return;

            window.Classes.Add("DoAFlip");

            DispatcherTimer.RunOnce(() => { window.Classes.Remove("DoAFlip"); }, TimeSpan.FromSeconds(1));
        });
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

    private async void ExportCustomTheme(object? sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider == null || DataContext is not OptionsTabViewModel vm) return;
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions { SuggestedFileName = "custom-theme.json", DefaultExtension = "json" });
        if (file == null) return;
        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(vm.ExportCustomThemeJson());
    }

    private async void ImportCustomTheme(object? sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider == null || DataContext is not OptionsTabViewModel vm) return;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { AllowMultiple = false, FileTypeFilter = new[] { new FilePickerFileType("JSON") { Patterns = new[] { "*.json" } } } });
        if (files.Count == 0) return;
        await using var stream = await files[0].OpenReadAsync();
        using var reader = new StreamReader(stream);
        vm.TryImportCustomThemeJson(await reader.ReadToEndAsync());
    }

    private void ResetCustomTheme(object? sender, RoutedEventArgs args) => (DataContext as OptionsTabViewModel)?.ResetCustomTheme();

    private async void PickCustomFont(object? sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider == null || DataContext is not OptionsTabViewModel vm) return;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { AllowMultiple = false, FileTypeFilter = new[] { new FilePickerFileType("Fonts") { Patterns = new[] { "*.ttf", "*.otf", "*.ttc" } } } });
        var path = files.Count == 0 ? null : files[0].TryGetLocalPath();
        if (!string.IsNullOrWhiteSpace(path)) vm.ApplyCustomFontFile(path);
    }

    private void ReloadResourcePacks(object? sender, RoutedEventArgs args) => (DataContext as OptionsTabViewModel)?.ReloadResourcePacks();
    private void OpenResourcePacksDirectory(object? sender, RoutedEventArgs args) => (DataContext as OptionsTabViewModel)?.OpenResourcePacksDirectory();
    private void ResourcePackEnabledChanged(object? sender, RoutedEventArgs args) => (DataContext as OptionsTabViewModel)?.SaveResourcePacks();
    private void MoveResourcePackUp(object? sender, RoutedEventArgs args) { if (sender is Button { DataContext: ResourcePackInfo pack }) (DataContext as OptionsTabViewModel)?.MoveResourcePack(pack, -1); }
    private void MoveResourcePackDown(object? sender, RoutedEventArgs args) { if (sender is Button { DataContext: ResourcePackInfo pack }) (DataContext as OptionsTabViewModel)?.MoveResourcePack(pack, 1); }
    private void ReloadKeybindConfigs(object? sender, RoutedEventArgs args) => (DataContext as OptionsTabViewModel)?.ReloadKeybindConfigs();
    private void OpenKeybindConfigsDirectory(object? sender, RoutedEventArgs args) => (DataContext as OptionsTabViewModel)?.OpenKeybindConfigsDirectory();
    private void ImportCurrentKeybinds(object? sender, RoutedEventArgs args) => (DataContext as OptionsTabViewModel)?.ImportCurrentKeybinds();
    private void ClearKeybindConfigSelection(object? sender, RoutedEventArgs args) => (DataContext as OptionsTabViewModel)?.ClearKeybindConfigSelection();
    private void SelectKeybindConfig(object? sender, RoutedEventArgs args) { if (sender is Button { DataContext: KeybindConfigInfo config }) (DataContext as OptionsTabViewModel)?.SelectKeybindConfig(config); }
    private void DeleteKeybindConfig(object? sender, RoutedEventArgs args) { if (sender is Button { DataContext: KeybindConfigInfo config }) (DataContext as OptionsTabViewModel)?.DeleteKeybindConfig(config); }
}
