using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SS14.Launcher.Models.Helix;
using SS14.Launcher.Utility;
using SS14.Launcher.ViewModels;

namespace SS14.Launcher.Views.Helix;

public sealed partial class MainWindowContentHelix : UserControl
{
    private MainWindowViewModel? _viewModel;

    public MainWindowContentHelix()
    {
        InitializeComponent();
        HelixDiscordRichPresence.Instance.SetActivity("Starting launcher");
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        }

        _viewModel = DataContext as MainWindowViewModel;

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
            UpdateSelectedTab();
            UpdateDiscordPresence();
        }

        base.OnDataContextChanged(e);
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedIndex) ||
            e.PropertyName == nameof(MainWindowViewModel.LoggedIn))
        {
            UpdateSelectedTab();
        }

        if (e.PropertyName == nameof(MainWindowViewModel.SelectedIndex) ||
            e.PropertyName == nameof(MainWindowViewModel.LoggedIn) ||
            e.PropertyName == nameof(MainWindowViewModel.ConnectingVM) ||
            e.PropertyName == nameof(MainWindowViewModel.BusyTask))
        {
            UpdateDiscordPresence();
        }
    }

    private void UpdateSelectedTab()
    {
        if (_viewModel == null || _viewModel.Tabs.Count == 0)
        {
            SelectedTabContent.Content = null;
            return;
        }

        var selectedIndex = Math.Clamp(_viewModel.SelectedIndex, 0, _viewModel.Tabs.Count - 1);
        SelectedTabContent.Content = _viewModel.Tabs[selectedIndex];

        NavHomeButton.Classes.Set("selected", selectedIndex == 0);
        NavServersButton.Classes.Set("selected", selectedIndex == 1);
        NavNewsButton.Classes.Set("selected", selectedIndex == 2);
        NavPatchesButton.Classes.Set("selected", selectedIndex == 3);
        NavOptionsButton.Classes.Set("selected", selectedIndex == 4);
    }

    private void NavHomeClicked(object? sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
            _viewModel.SelectedIndex = 0;
    }

    private void NavServersClicked(object? sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
            _viewModel.SelectedIndex = 1;
    }

    private void NavNewsClicked(object? sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
            _viewModel.SelectedIndex = 2;
    }

    private void NavPatchesClicked(object? sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
            _viewModel.SelectedIndex = 3;
    }

    private void NavOptionsClicked(object? sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
            _viewModel.SelectedIndex = 4;
    }

    private void HelixDiscordClicked(object? sender, RoutedEventArgs e)
    {
        Helpers.OpenUri(new Uri(HelixDiscordRichPresence.DiscordUrl));
    }

    private void UpdateDiscordPresence()
    {
        if (_viewModel == null)
        {
            HelixDiscordRichPresence.Instance.SetActivity("Starting launcher");
            return;
        }

        if (_viewModel.ConnectingVM != null)
        {
            HelixDiscordRichPresence.Instance.SetActivity("Launching a server");
            return;
        }

        if (!_viewModel.LoggedIn)
        {
            HelixDiscordRichPresence.Instance.SetActivity(
                string.IsNullOrWhiteSpace(_viewModel.BusyTask)
                    ? "At login screen"
                    : _viewModel.BusyTask);
            return;
        }

        if (_viewModel.Tabs.Count == 0)
        {
            HelixDiscordRichPresence.Instance.SetActivity("In launcher");
            return;
        }

        var selectedIndex = Math.Clamp(_viewModel.SelectedIndex, 0, _viewModel.Tabs.Count - 1);
        var state = selectedIndex switch
        {
            0 => "Viewing home",
            1 => "Browsing servers",
            2 => "Reading news",
            3 => "Managing resource packs",
            4 => "Changing settings",
            _ => $"Viewing {_viewModel.Tabs[selectedIndex].Name}"
        };

        HelixDiscordRichPresence.Instance.SetActivity(state);
    }
}
