using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI;
using TerraFX.Interop.Windows;

namespace SS14.Launcher.Views.Worm;

public partial class DirectConnectDialogWorm : Window
{
    private readonly TextBox _addressBox;

    public DirectConnectDialogWorm()
    {
        InitializeComponent();
        
        DarkMode();
        

        _addressBox = AddressBox;
        _addressBox.KeyDown += (_, args) =>
        {
            if (args.Key == Key.Enter)
            {
                TrySubmit();
            }
        };

        SubmitButton.Command = ReactiveCommand.Create(TrySubmit);

        this.WhenAnyValue(x => x._addressBox.Text)
            .Select(IsAddressValid)
            .Subscribe(b =>
            {
                InvalidLabel.IsVisible = !b;
                SubmitButton.IsEnabled = b;
            });
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        _addressBox.Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close(null);
        }

        base.OnKeyDown(e);
    }

    private void TrySubmit()
    {
        if (!IsAddressValid(_addressBox.Text))
        {
            return;
        }

        Close(_addressBox.Text.Trim());
    }

    internal static bool IsAddressValid([NotNullWhen(true)] string? address)
    {
        return !string.IsNullOrWhiteSpace(address) && UriHelper.TryParseSs14Uri(address, out _);
    }

    
    private unsafe void DarkMode()
    {
        if (!OperatingSystem.IsWindows() || Environment.OSVersion.Version.Build < 22000)
            return;

        if (TryGetPlatformHandle() is not { HandleDescriptor: "HWND" } handle)
            return;

        var hWnd = (HWND)handle.Handle;
        COLORREF caption = 0x001C140F;
        COLORREF border = 0x0035291F;
        COLORREF text = 0x00F0E9E5;
        TerraFX.Interop.Windows.Windows.DwmSetWindowAttribute(hWnd, 35, &caption, (uint) sizeof(COLORREF));
        TerraFX.Interop.Windows.Windows.DwmSetWindowAttribute(hWnd, 34, &border, (uint) sizeof(COLORREF));
        TerraFX.Interop.Windows.Windows.DwmSetWindowAttribute(hWnd, 36, &text, (uint) sizeof(COLORREF));
        Classes.Add("WindowsTitlebarColorActive");
    }
    
}

