using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.VisualTree;
using DynamicData;
using DynamicData.Alias;
using Microsoft.Toolkit.Mvvm.Messaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using SS14.Launcher.Localization;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.ServerStatus;
using SS14.Launcher.Models.Worm;
using SS14.Launcher.Utility;
using SS14.Launcher.Views;
using SS14.Launcher.Views.Worm;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public class HomePageViewModel : MainWindowTabViewModel
{
    public MainWindowViewModel MainWindowViewModel { get; }
    private readonly DataManager _cfg;
    private readonly ServerStatusCache _statusCache = new ServerStatusCache();
    private readonly ServerListCache _serverListCache;
    // Worm-Start
    private readonly RecentServerManager _recentServerManager;
    // Worm-End

    public HomePageViewModel(MainWindowViewModel mainWindowViewModel)
    {
        MainWindowViewModel = mainWindowViewModel;
        _cfg = Locator.Current.GetRequiredService<DataManager>();
        _serverListCache = Locator.Current.GetRequiredService<ServerListCache>();
        // Worm-Start
        _recentServerManager = Locator.Current.GetRequiredService<RecentServerManager>();
        _recentServerManager.Entries.CollectionChanged += (_, _) => RebuildRecentServers();
        // Worm-End
        // Worm-Start
        WeakReferenceMessenger.Default.Register<ServerListDisplaySettingsChanged>(this, (_, _) => RaiseServerListDisplayPropertiesChanged());
        // Worm-End

        _cfg.FavoriteServers
            .Connect()
            .Select(x => new ServerEntryViewModel(MainWindowViewModel, _statusCache.GetStatusFor(x.Address), x, _statusCache, _cfg) { ViewedInFavoritesPane = true })
            .OnItemAdded(a =>
            {
                if (IsSelected)
                {
                    _statusCache.InitialUpdateStatus(a.CacheData);
                    PreloadInfo(a);
                }
            })
            .Sort(Comparer<ServerEntryViewModel>.Create((a, b) => {
                var dc = a.Favorite!.RaiseTime.CompareTo(b.Favorite!.RaiseTime);
                if (dc != 0)
                {
                    return -dc;
                }
                return string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase);
            }))
            .Bind(out var favorites)
            .Subscribe(_ =>
            {
                FavoritesEmpty = favorites.Count == 0;
            });

        Favorites = favorites;
        // Worm-Start
        RebuildRecentServers();
        // Worm-End
    }

    public ReadOnlyObservableCollection<ServerEntryViewModel> Favorites { get; }
    public ObservableCollection<ServerEntryViewModel> Suggestions { get; } = new();
    // Worm-Start
    public ObservableCollection<ServerEntryViewModel> RecentServers { get; } = new();
    // Worm-End

    [Reactive] public bool FavoritesEmpty { get; private set; } = true;
    // Worm-Start
    [Reactive] public bool RecentServersEmpty { get; private set; } = true;
    // Worm-End

    public override string Name => LocalizationManager.Instance.GetString("tab-home-title");
    public Control? Control { get; set; }
    // Worm-Start
    public bool ShowMapColumn => _cfg.GetCVar(CVars.ServerListShowMap);
    public bool ShowModeColumn => _cfg.GetCVar(CVars.ServerListShowMode);
    public bool ShowPingColumn => _cfg.GetCVar(CVars.ServerListShowPing);
    // Worm-End

    public async void DirectConnectPressed()
    {
        if (!TryGetWindow(out var window))
        {
            return;
        }

        // Worm-Start
        var res = await new DirectConnectDialogWorm().ShowDialog<string?>(window);
        // Worm-End
        if (res == null)
        {
            return;
        }

        ConnectingViewModel.StartConnect(MainWindowViewModel, res);
    }

    public async void AddFavoritePressed()
    {
        if (!TryGetWindow(out var window))
        {
            return;
        }

        // Worm-Start
        var (name, address) = await new AddFavoriteDialogWorm().ShowDialog<(string name, string address)>(window);
        // Worm-End

        try
        {
            _cfg.AddFavoriteServer(new FavoriteServer(name, address));
            _cfg.CommitConfig();
        }
        catch (ArgumentException)
        {
            // Happens if address already a favorite, so ignore.
            // TODO: Give a popup to the user?
        }
    }

    private bool TryGetWindow([NotNullWhen(true)] out Window? window)
    {
        window = Control?.GetVisualRoot() as Window;
        return window != null;
    }

    public void RefreshPressed()
    {
        _statusCache.Refresh();
        _serverListCache.RequestRefresh();
    }

    // Worm-Start
    public void ClearRecentServersPressed()
    {
        _recentServerManager.Clear();
    }
    // Worm-End

    public override void Selected()
    {
        foreach (var favorite in Favorites)
        {
            _statusCache.InitialUpdateStatus(favorite.CacheData);
            PreloadInfo(favorite);
        }

        // Worm-Start
        foreach (var recent in RecentServers)
        {
            _statusCache.InitialUpdateStatus(recent.CacheData);
            PreloadInfo(recent);
        }
        // Worm-End

        _serverListCache.RequestInitialUpdate();
    }

    // Worm-Start
    private void RaiseServerListDisplayPropertiesChanged()
    {
        this.RaisePropertyChanged(nameof(ShowMapColumn));
        this.RaisePropertyChanged(nameof(ShowModeColumn));
        this.RaisePropertyChanged(nameof(ShowPingColumn));
    }
    // Worm-End

    // Worm-Start
    private void RebuildRecentServers()
    {
        RecentServers.Clear();

        foreach (var entry in _recentServerManager.Entries)
        {
            var statusData = _statusCache.GetStatusFor(entry.Address);
            var vm = new ServerEntryViewModel(
                MainWindowViewModel,
                new ServerStatusDataWithFallbackName(statusData, entry.DisplayName),
                _statusCache,
                _cfg);

            RecentServers.Add(vm);

            if (IsSelected)
            {
                _statusCache.InitialUpdateStatus(vm.CacheData);
                PreloadInfo(vm);
            }
        }

        RecentServersEmpty = RecentServers.Count == 0;
    }

    private void PreloadInfo(ServerEntryViewModel viewModel)
    {
        if (viewModel.CacheData.Status != ServerStatusCode.Online)
            return;

        if (viewModel.CacheData.StatusInfo is not (ServerStatusInfoCode.NotFetched or ServerStatusInfoCode.Error))
            return;

        ((IServerSource)_statusCache).UpdateInfoFor(viewModel.CacheData);
    }
    // Worm-End
}
