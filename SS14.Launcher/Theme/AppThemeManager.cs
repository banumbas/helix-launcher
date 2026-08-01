using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SS14.Launcher.Theme;

public enum LauncherTheme
{
    Dark,
    Light,
    DarkRed,
    DarkPurple,
    MidnightBlue,
    EmeraldDusk,
    CopperNight,
    Custom
}

public static class AppThemeManager
{
    public const string DefaultFontDescriptor = "avares://SS14.Launcher/Assets/Fonts/noto_sans/*.ttf#Noto Sans";
    public sealed record CustomThemeColors(Color Background, Color Accent, Color Foreground, Color Popup, Color GradientStart, Color GradientEnd);

    private sealed record Palette(Color Background, Color Popup, Color Foreground, Color Accent, Color ControlMid, Color SubText);

    public static LauncherTheme Normalize(int value) => Enum.IsDefined(typeof(LauncherTheme), value)
        ? (LauncherTheme)value
        : LauncherTheme.Dark;

    public static bool ApplyFont(Application app, string? descriptor)
    {
        try
        {
            app.Resources["ThemeFontFamily"] = new FontFamily(string.IsNullOrWhiteSpace(descriptor) ? DefaultFontDescriptor : descriptor);
            return true;
        }
        catch
        {
            app.Resources["ThemeFontFamily"] = new FontFamily(DefaultFontDescriptor);
            return false;
        }
    }

    public static void ApplyTheme(Application app, LauncherTheme theme, bool gradientEnabled, bool decorEnabled, CustomThemeColors custom)
    {
        var palette = GetPalette(theme, custom);
        var hover = Mix(palette.ControlMid, palette.Accent, 0.22);
        var borderMid = Mix(palette.Popup, palette.Foreground, 0.18);
        var borderHigh = Mix(palette.Accent, palette.Foreground, 0.30);

        Set(app, "ThemeBackgroundColor", palette.Background);
        Set(app, "ThemePopupBackgroundColor", palette.Popup);
        Set(app, "ThemeForegroundColor", palette.Foreground);
        Set(app, "ThemeForegroundMutedColor", Mix(palette.Foreground, palette.Background, 0.58));
        Set(app, "ThemeControlMidColor", palette.ControlMid);
        Set(app, "ThemeControlHighColor", palette.Accent);
        Set(app, "ThemeNanoGoldColor", palette.Accent);
        Set(app, "ThemeSubTextColor", palette.SubText);
        Set(app, "ThemeStripebackEdgeColor", Mix(palette.Background, palette.Popup, 0.45));
        Set(app, "ThemeButtonHoveredColor", hover);
        Set(app, "ThemeTabItemSelectedColor", WithAlpha(palette.Accent, 0xCC));
        Set(app, "ThemeTabItemHoveredColor", WithAlpha(hover, 0xAA));
        Set(app, "ThemeListSeparatorColor", WithAlpha(borderHigh, 0xAA));
        Set(app, "ThemeListSeparatorColorTransparent", WithAlpha(borderHigh, 0));
        Set(app, "ThemeBorderMidColor", borderMid);
        Set(app, "ThemeBorderHighColor", borderHigh);
        Set(app, "ThemeListAltRowColor", Mix(palette.Background, palette.Popup, 0.28));

        SetBrush(app, "ThemePopupBackgroundBrush", palette.Popup);
        SetBrush(app, "ThemeForegroundBrush", palette.Foreground);
        SetBrush(app, "ThemeForegroundMutedBrush", Mix(palette.Foreground, palette.Background, 0.58));
        SetBrush(app, "ThemeControlMidBrush", palette.ControlMid);
        SetBrush(app, "ThemeControlHighBrush", palette.Accent);
        SetBrush(app, "ThemeNanoGoldBrush", palette.Accent);
        SetBrush(app, "ThemeSubTextBrush", palette.SubText);
        SetBrush(app, "ThemeButtonHoveredBrush", hover);
        SetBrush(app, "ThemeStripebackEdgeBrush", Mix(palette.Background, palette.Popup, 0.45));
        SetBrush(app, "ThemeTabItemSelectedBrush", WithAlpha(palette.Accent, 0xCC));
        SetBrush(app, "ThemeTabItemHoveredBrush", WithAlpha(hover, 0xAA));
        SetBrush(app, "ThemeListAltRowBrush", Mix(palette.Background, palette.Popup, 0.28));

        if (gradientEnabled)
        {
            var start = theme == LauncherTheme.Custom ? Opaque(custom.GradientStart) : Mix(palette.Background, palette.Popup, 0.18);
            var end = theme == LauncherTheme.Custom ? Opaque(custom.GradientEnd) : Mix(palette.Background, palette.Accent, 0.28);
            app.Resources["ThemeBackgroundBrush"] = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops { new(start, 0), new(Mix(start, end, .5), .5), new(end, 1) }
            };
        }
        else
        {
            SetBrush(app, "ThemeBackgroundBrush", palette.Background);
        }

        app.Resources["ThemeStripeBackBrush"] = decorEnabled
            ? CreateStripeBrush(palette.Background, Mix(palette.Background, palette.Popup, .28))
            : new SolidColorBrush(palette.Background);
        app.Resources["WindowOverlayBrush"] = new SolidColorBrush(Color.Parse(theme == LauncherTheme.Light ? "#66000000" : "#AA000000"));
    }

    private static Palette GetPalette(LauncherTheme theme, CustomThemeColors custom)
    {
        return theme switch
        {
            LauncherTheme.Light => new(Color.Parse("#F6F8FC"), Color.Parse("#FFFFFF"), Color.Parse("#1B2230"), Color.Parse("#2E7A66"), Color.Parse("#E6ECF5"), Color.Parse("#6B778A")),
            LauncherTheme.DarkRed => new(Color.Parse("#1E1719"), Color.Parse("#231A1D"), Color.Parse("#F0E8EA"), Color.Parse("#8A3B48"), Color.Parse("#4A3136"), Color.Parse("#B9A5AA")),
            LauncherTheme.DarkPurple => new(Color.Parse("#1C1824"), Color.Parse("#211C2C"), Color.Parse("#EEE9F7"), Color.Parse("#6F4EA1"), Color.Parse("#41355A"), Color.Parse("#B9AFCD")),
            LauncherTheme.MidnightBlue => new(Color.Parse("#141B24"), Color.Parse("#18212C"), Color.Parse("#E9F1F9"), Color.Parse("#3C7FA6"), Color.Parse("#2C4158"), Color.Parse("#A9BED1")),
            LauncherTheme.EmeraldDusk => new(Color.Parse("#12211D"), Color.Parse("#162A25"), Color.Parse("#E7F6F0"), Color.Parse("#2F8F73"), Color.Parse("#27473E"), Color.Parse("#A8C6BA")),
            LauncherTheme.CopperNight => new(Color.Parse("#211915"), Color.Parse("#2A201A"), Color.Parse("#F6ECE5"), Color.Parse("#B06A45"), Color.Parse("#5B4033"), Color.Parse("#CAB4A8")),
            LauncherTheme.Custom => new(Opaque(custom.Background), Opaque(custom.Popup), Opaque(custom.Foreground), Opaque(custom.Accent), Mix(custom.Background, custom.Popup, .62), Mix(custom.Foreground, custom.Background, .46)),
            _ => new(Color.Parse("#25252A"), Color.Parse("#202025"), Color.Parse("#EEEEEE"), Color.Parse("#3E6C45"), Color.Parse("#464966"), Color.Parse("#AAAAAA"))
        };
    }

    private static void Set(Application app, string key, Color color) => app.Resources[key] = color;
    private static void SetBrush(Application app, string key, Color color) => app.Resources[key] = new SolidColorBrush(color);
    private static Color Opaque(Color color) => new(255, color.R, color.G, color.B);
    private static Color WithAlpha(Color color, byte alpha) => new(alpha, color.R, color.G, color.B);
    private static Color Mix(Color a, Color b, double amount) => new(255, (byte)(a.R + (b.R - a.R) * amount), (byte)(a.G + (b.G - a.G) * amount), (byte)(a.B + (b.B - a.B) * amount));

    private static IBrush CreateStripeBrush(Color background, Color stripe) => new VisualBrush
    {
        Visual = new Panel
        {
            Width = 32, Height = 32, Background = new SolidColorBrush(background),
            Children = { new Avalonia.Controls.Shapes.Path { Data = Geometry.Parse("M 0 8 L 24 32 L 8 32 L 0 24 Z"), Fill = new SolidColorBrush(stripe) }, new Avalonia.Controls.Shapes.Path { Data = Geometry.Parse("M 8 0 L 24 0 L 32 8 L 32 24 Z"), Fill = new SolidColorBrush(stripe) } }
        },
        TileMode = TileMode.Tile, Stretch = Stretch.Fill,
        SourceRect = new RelativeRect(0, 0, 32, 32, RelativeUnit.Absolute),
        DestinationRect = new RelativeRect(0, 0, 32, 32, RelativeUnit.Absolute)
    };
}
