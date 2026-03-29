using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using SS14.Launcher.ViewModels.MainWindowTabs;

namespace SS14.Launcher.Views.Helix;

public sealed partial class ServerEntryHelixView : UserControl
{
    public ServerEntryHelixView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is ObservableRecipient recipient)
            recipient.IsActive = true;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (DataContext is ObservableRecipient recipient)
            recipient.IsActive = false;
    }

    private void ServerRowPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        if (DataContext is not ServerEntryViewModel viewModel)
            return;

        if (e.Source is Visual visual && visual.FindAncestorOfType<Button>() != null)
            return;

        viewModel.IsExpanded = !viewModel.IsExpanded;
        e.Handled = true;
    }

    private void ConnectClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ServerEntryViewModel viewModel)
            viewModel.ConnectPressed();
    }

    private void FavoriteClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ServerEntryViewModel viewModel)
            viewModel.FavoriteButtonPressed();
    }

    private void RaiseFavoriteClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ServerEntryViewModel viewModel)
            viewModel.FavoriteRaiseButtonPressed();
    }
}
