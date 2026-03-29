using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.ViewModels;

namespace SS14.Launcher.ViewModels.Worm;

public sealed class WormStoredAccountViewModel : ViewModelBase
{
    public extern string StatusText { [ObservableAsProperty] get; }

    public LoggedInAccount Account { get; }

    public WormStoredAccountViewModel(LoggedInAccount account)
    {
        Account = account;

        this.WhenAnyValue<WormStoredAccountViewModel, AccountLoginStatus, string>(
                p => p.Account.Status,
                p => p.Account.Username)
            .Select(p => p.Item1 switch
            {
                AccountLoginStatus.Available => p.Item2,
                AccountLoginStatus.Expired => $"{p.Item2} (!)",
                _ => $"{p.Item2} (?)"
            })
            .ToPropertyEx(this, x => x.StatusText);
    }
}
