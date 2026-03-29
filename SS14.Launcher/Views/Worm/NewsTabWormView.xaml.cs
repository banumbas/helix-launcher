using Avalonia.Controls;
using Avalonia.Interactivity;
using SS14.Launcher.ViewModels.MainWindowTabs;

namespace SS14.Launcher.Views.Worm;

public sealed partial class NewsTabWormView : UserControl
{
    public NewsTabWormView()
    {
        InitializeComponent();
    }

    private static void NewsEntryPressed(object? sender, RoutedEventArgs args)
    {
        if (sender is Button { DataContext: NewsEntryViewModel entry })
        {
            entry.Open();
        }
    }
}
