using System;
using Avalonia.Controls;
using SS14.Launcher.ViewModels;
using SS14.Launcher.ViewModels.Worm;

namespace SS14.Launcher.Views.Worm;

public partial class AccountDropDownWorm : UserControl
{
    private AccountDropDownWormViewModel? _viewModel;

    public AccountDropDownWorm()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is MainWindowViewModel mainWindowViewModel &&
            !ReferenceEquals(_viewModel, DropDownRoot.DataContext))
        {
            _viewModel = new AccountDropDownWormViewModel(mainWindowViewModel);
            DropDownRoot.DataContext = _viewModel;
        }

        base.OnDataContextChanged(e);
    }
}
