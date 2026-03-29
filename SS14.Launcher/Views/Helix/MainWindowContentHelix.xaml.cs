using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SS14.Launcher.ViewModels;

namespace SS14.Launcher.Views.Helix;

public sealed partial class MainWindowContentHelix : UserControl
{
    private MainWindowViewModel? _viewModel;

    public MainWindowContentHelix()
    {
        InitializeComponent();
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
}
