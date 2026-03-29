using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DynamicData;
using Serilog;
using Splat;
using SS14.Launcher.Api;
using SS14.Launcher.Localization;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.Utility;
using SS14.Launcher.ViewModels;
using SS14.Launcher.ViewModels.Login;
using SS14.Launcher.ViewModels.Worm;

namespace SS14.Launcher.Views.Worm;

public partial class MainWindowLoginWorm : UserControl
{
    private readonly LoginManager _loginMgr;
    private readonly LocalizationManager _loc;
    private MainWindowLoginViewModel? _viewModel;

    public MainWindowLoginWorm()
    {
        InitializeComponent();

        _loginMgr = Locator.Current.GetRequiredService<LoginManager>();
        _loc = LocalizationManager.Instance;

        _loginMgr.Logins
            .Connect()
            .Transform(account => new WormStoredAccountViewModel(account))
            .Bind(out var accounts)
            .Subscribe(_ => UpdateStoredAccountsState(accounts));

        StoredAccountsList.ItemsSource = accounts;
        UpdateStoredAccountsState(accounts);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        _viewModel = DataContext as MainWindowLoginViewModel;
        base.OnDataContextChanged(e);
    }

    private void StoredAccountClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: LoggedInAccount account })
            return;

        TrySwitchToStoredAccount(account);
    }

    private void UpdateStoredAccountsState(System.Collections.Generic.IReadOnlyCollection<WormStoredAccountViewModel> accounts)
    {
        StoredAccountsSection.IsVisible = accounts.Count > 0;
    }

    private void TrySwitchToStoredAccount(LoggedInAccount account)
    {
        if (_viewModel == null)
            return;

        switch (account.Status)
        {
            case AccountLoginStatus.Unsure:
                TrySelectUnsureAccount(account);
                break;

            case AccountLoginStatus.Available:
                _loginMgr.ActiveAccount = account;
                break;

            case AccountLoginStatus.Expired:
                _loginMgr.ActiveAccount = null;
                _viewModel.SwitchToExpiredLogin(account);
                break;
        }
    }

    private async void TrySelectUnsureAccount(LoggedInAccount account)
    {
        if (_viewModel == null)
            return;

        var screen = _viewModel.Screen;
        try
        {
            await _loginMgr.UpdateSingleAccountStatus(account);
            TrySwitchToStoredAccount(account);
        }
        catch (AuthApiException ex)
        {
            Log.Warning(ex, "AuthApiException while trying to refresh account {login}", account.LoginInfo);
            screen.OverlayControl = new AuthErrorsOverlayViewModel(
                screen,
                _loc.GetString("main-window-error-connecting-auth-server"),
                new[]
                {
                    ex.InnerException?.Message ?? _loc.GetString("main-window-error-unknown")
                });
        }
    }
}
