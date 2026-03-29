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
using SS14.Launcher.Models.Helix;
using SS14.Launcher.Utility;
using SS14.Launcher.Views;
using SS14.Launcher.Views.Helix;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public class HomePageViewModel : MainWindowTabViewModel
{
    public MainWindowViewModel MainWindowViewModel { get; }
    private readonly DataManager _cfg;
    private readonly ServerStatusCache _statusCache = new ServerStatusCache();
    private readonly ServerListCache _serverListCache;
    // Helix-Start
    private readonly RecentServerManager _recentServerManager;
    // Helix-End

    public HomePageViewModel(MainWindowViewModel mainWindowViewModel)
    {
        MainWindowViewModel = mainWindowViewModel;
        _cfg = Locator.Current.GetRequiredService<DataManager>();
        _serverListCache = Locator.Current.GetRequiredService<ServerListCache>();
        // Helix-Start
        _recentServerManager = Locator.Current.GetRequiredService<RecentServerManager>();
        _recentServerManager.Entries.CollectionChanged += (_, _) => RebuildRecentServers();
        // Helix-End
        // Helix-Start
        WeakReferenceMessenger.Default.Register<ServerListDisplaySettingsChanged>(this, (_, _) => RaiseServerListDisplayPropertiesChanged());
        // Helix-End

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
        // Helix-Start
        RebuildRecentServers();
        // Helix-End
    }

    public ReadOnlyObservableCollection<ServerEntryViewModel> Favorites { get; }
    public ObservableCollection<ServerEntryViewModel> Suggestions { get; } = new();
    // Helix-Start
    public ObservableCollection<ServerEntryViewModel> RecentServers { get; } = new();
    // Helix-End

    [Reactive] public bool FavoritesEmpty { get; private set; } = true;
    // Helix-Start
    [Reactive] public bool RecentServersEmpty { get; private set; } = true;
    // Helix-End

    public override string Name => LocalizationManager.Instance.GetString("tab-home-title");
    public Control? Control { get; set; }
    // Helix-Start
    public bool ShowMapColumn => _cfg.GetCVar(CVars.ServerListShowMap);
    public bool ShowModeColumn => _cfg.GetCVar(CVars.ServerListShowMode);
    public bool ShowPingColumn => _cfg.GetCVar(CVars.ServerListShowPing);
    // Helix-End

    public async void DirectConnectPressed()
    {
        if (!TryGetWindow(out var window))
        {
            return;
        }

        // Helix-Start
        var res = await new DirectConnectDialogHelix().ShowDialog<string?>(window);
        // Helix-End
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

        // Helix-Start
        var (name, address) = await new AddFavoriteDialogHelix().ShowDialog<(string name, string address)>(window);
        // Helix-End

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

    // Helix-Start
    public void ClearRecentServersPressed()
    {
        _recentServerManager.Clear();
    }
    // Helix-End

    public override void Selected()
    {
        foreach (var favorite in Favorites)
        {
            _statusCache.InitialUpdateStatus(favorite.CacheData);
            PreloadInfo(favorite);
        }

        // Helix-Start
        foreach (var recent in RecentServers)
        {
            _statusCache.InitialUpdateStatus(recent.CacheData);
            PreloadInfo(recent);
        }
        // Helix-End

        _serverListCache.RequestInitialUpdate();
    }

    // Helix-Start
    private void RaiseServerListDisplayPropertiesChanged()
    {
        this.RaisePropertyChanged(nameof(ShowMapColumn));
        this.RaisePropertyChanged(nameof(ShowModeColumn));
        this.RaisePropertyChanged(nameof(ShowPingColumn));
    }
    // Helix-End

    // Helix-Start
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
    // Helix-End
}
