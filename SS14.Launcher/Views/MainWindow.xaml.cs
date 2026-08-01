using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using SS14.Launcher.Localization;
using SS14.Launcher.ViewModels;
using TerraFX.Interop.Windows;
using IDataObject = Avalonia.Input.IDataObject;

namespace SS14.Launcher.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;

    private MainWindowContent _content;

    public MainWindow()
    {
        InitializeComponent();

        DarkMode();

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DragOverEvent, DragOver);
        AddHandler(DragDrop.DropEvent, Drop);

        _content = (MainWindowContent) Content!;

        ReloadTitle();
    }

    public void ReloadContent()
    {
        ReloadTitle();

        Content = _content = new MainWindowContent();
    }

    private void ReloadTitle()
    {
        Title = LocalizationManager.Instance.GetString("main-window-title");
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.Control = null;
        }

        _viewModel = DataContext as MainWindowViewModel;

        if (_viewModel != null)
        {
            _viewModel.Control = this;
        }

        base.OnDataContextChanged(e);
    }

    private unsafe void DarkMode()
    {
        if (!OperatingSystem.IsWindows() || Environment.OSVersion.Version.Build < 22000)
            return;

        if (TryGetPlatformHandle() is not { HandleDescriptor: "HWND" } handle)
        {
            // No need to log a warning, PJB will notice when this breaks.
            return;
        }

        RefreshTitleBarColors();

        // Removes the top margin of the window on Windows 11, since there's ample space after we recolor the title bar.
        Classes.Add("WindowsTitlebarColorActive");
    }

    /// <summary>Updates the native Windows title bar after a launcher theme change.</summary>
    public unsafe void RefreshTitleBarColors()
    {
        if (!OperatingSystem.IsWindows() || Environment.OSVersion.Version.Build < 22000)
            return;

        if (TryGetPlatformHandle() is not { HandleDescriptor: "HWND" } handle)
            return;

        var background = GetThemeColor("ThemeBackgroundColor", Color.Parse("#25252A"));
        var foreground = GetThemeColor("ThemeForegroundColor", Color.Parse("#EEEEEE"));
        var border = Mix(background, foreground, 0.18);
        var hWnd = (HWND)handle.Handle;
        var caption = ToColorRef(background);
        var borderColor = ToColorRef(border);
        var text = ToColorRef(foreground);

        TerraFX.Interop.Windows.Windows.DwmSetWindowAttribute(hWnd, 35, &caption, (uint) sizeof(COLORREF));
        TerraFX.Interop.Windows.Windows.DwmSetWindowAttribute(hWnd, 34, &borderColor, (uint) sizeof(COLORREF));
        TerraFX.Interop.Windows.Windows.DwmSetWindowAttribute(hWnd, 36, &text, (uint) sizeof(COLORREF));
    }

    private static Color GetThemeColor(string key, Color fallback) => Application.Current?.Resources[key] is Color color ? color : fallback;
    private static Color Mix(Color a, Color b, double amount) => new(255, (byte)(a.R + (b.R - a.R) * amount), (byte)(a.G + (b.G - a.G) * amount), (byte)(a.B + (b.B - a.B) * amount));
    private static COLORREF ToColorRef(Color color) => (COLORREF)(uint)(color.R | color.G << 8 | color.B << 16);

    private void Drop(object? sender, DragEventArgs args)
    {
        _content.DragDropOverlay.IsVisible = false;

        if (!IsDragDropValid(args.Data))
            return;

        var file = GetDragDropFile(args.Data)!;
        _viewModel!.Dropped(file);
    }

    private void DragOver(object? sender, DragEventArgs args)
    {
        if (!IsDragDropValid(args.Data))
        {
            args.DragEffects = DragDropEffects.None;
            return;
        }

        args.DragEffects = DragDropEffects.Link;
    }

    private void DragLeave(object? sender, RoutedEventArgs args)
    {
        _content.DragDropOverlay.IsVisible = false;
    }

    private void DragEnter(object? sender, DragEventArgs args)
    {
        if (!IsDragDropValid(args.Data))
            return;

        _content.DragDropOverlay.IsVisible = true;
    }

    private bool IsDragDropValid(IDataObject dataObject)
    {
        if (_viewModel == null)
            return false;

        if (GetDragDropFile(dataObject) is not { } fileName)
            return false;

        return _viewModel.IsContentBundleDropValid(fileName);
    }

    private static IStorageFile? GetDragDropFile(IDataObject dataObject)
    {
        if (!dataObject.Contains(DataFormats.Files))
            return null;

        return dataObject.GetFiles()?.OfType<IStorageFile>().FirstOrDefault();
    }
}
