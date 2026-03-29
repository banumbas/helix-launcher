using System;
using Avalonia.Controls;
using SS14.Launcher.ViewModels;
using SS14.Launcher.ViewModels.Helix;

namespace SS14.Launcher.Views.Helix;

public partial class AccountDropDownHelix : UserControl
{
    private AccountDropDownHelixViewModel? _viewModel;

    public AccountDropDownHelix()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is MainWindowViewModel mainWindowViewModel &&
            !ReferenceEquals(_viewModel, DropDownRoot.DataContext))
        {
            _viewModel = new AccountDropDownHelixViewModel(mainWindowViewModel);
            DropDownRoot.DataContext = _viewModel;
        }

        base.OnDataContextChanged(e);
    }
}
