using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using Serilog;
using SS14.Launcher.ViewModels;
using SS14.Launcher.ViewModels.MainWindowTabs;

namespace SS14.Launcher.Views.Helix;

public sealed partial class HomePageHelixView : UserControl
{
    private HomePageViewModel? _viewModel;

    private HomePageViewModel? ViewModel => _viewModel;

    public HomePageHelixView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.Control = null;
        }

        _viewModel = DataContext as HomePageViewModel;

        if (_viewModel != null)
        {
            _viewModel.Control = this;
        }

        base.OnDataContextChanged(e);
    }

    private async void OpenReplayClicked(object? sender, RoutedEventArgs e)
    {
        if (_viewModel?.MainWindowViewModel is not { } mainVm)
            return;

        if (this.GetVisualRoot() is not Window window)
        {
            Log.Error("Visual root isn't a window!");
            return;
        }

        var result = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select replay or content bundle file",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Replay or content bundle files")
                {
                    Patterns = ["*.zip"],
                    MimeTypes = ["application/zip"],
                    AppleUniformTypeIdentifiers = ["zip"]
                }
            ]
        });

        if (result.Count == 0)
            return;

        using var file = result[0];
        if (!mainVm.IsContentBundleDropValid(file))
            return;

        ConnectingViewModel.StartContentBundle(mainVm, file);
    }

    private void RefreshClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel?.RefreshPressed();
    }

    private void DirectConnectClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel?.DirectConnectPressed();
    }

    private void AddFavoriteClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel?.AddFavoritePressed();
    }

    private void GoToServersClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel?.MainWindowViewModel.SelectTabServers();
    }

    private void ClearHistoryClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel?.ClearRecentServersPressed();
    }
}
