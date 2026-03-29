using System;
using Avalonia.Controls;
using ReactiveUI;
using TerraFX.Interop.Windows;

namespace SS14.Launcher.Views.Worm;

public partial class AddFavoriteDialogWorm : Window
{
    private readonly TextBox _nameBox;
    private readonly TextBox _addressBox;

    public AddFavoriteDialogWorm()
    {
        InitializeComponent();
        DarkMode();

        _nameBox = NameBox;
        _addressBox = AddressBox;

        SubmitButton.Command = ReactiveCommand.Create(TrySubmit);

        this.WhenAnyValue(x => x._nameBox.Text)
            .Subscribe(_ => UpdateSubmitValid());

        this.WhenAnyValue(x => x._addressBox.Text)
            .Subscribe(_ => UpdateSubmitValid());
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        _nameBox.Focus();
    }

    private void TrySubmit()
    {
        Close((_nameBox.Text?.Trim() ?? "", _addressBox.Text?.Trim() ?? ""));
    }

    private void UpdateSubmitValid()
    {
        var validAddr = DirectConnectDialogWorm.IsAddressValid(_addressBox.Text);
        var valid = validAddr && !string.IsNullOrEmpty(_nameBox.Text);

        SubmitButton.IsEnabled = valid;
        TxtInvalid.IsVisible = !validAddr;
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
