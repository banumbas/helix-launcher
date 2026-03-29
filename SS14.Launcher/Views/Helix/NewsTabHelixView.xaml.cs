using Avalonia.Controls;
using Avalonia.Interactivity;
using SS14.Launcher.ViewModels.MainWindowTabs;

namespace SS14.Launcher.Views.Helix;

public sealed partial class NewsTabHelixView : UserControl
{
    public NewsTabHelixView()
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
