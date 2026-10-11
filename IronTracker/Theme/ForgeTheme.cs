using MudBlazor;

namespace IronTracker.Theme;

/// <summary>"Forge" MudBlazor theme: dark industrial ground with molten ember accents.</summary>
public static class ForgeTheme
{
    private static readonly string[] HeadingFont = ["Barlow Condensed", "Arial Narrow", "sans-serif"];
    private static readonly string[] BodyFont = ["Barlow", "Helvetica Neue", "Arial", "sans-serif"];

    public static MudTheme Theme { get; } = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#FF6B2C",
            PrimaryContrastText = "#0F0F10",
            Secondary = "#FFB547",
            SecondaryContrastText = "#0F0F10",
            Tertiary = "#A1A1AA",
            Info = "#FFB547",
            Success = "#4ADE80",
            SuccessContrastText = "#0F0F10",
            Warning = "#FFB547",
            WarningContrastText = "#0F0F10",
            Error = "#F87171",
            Background = "#0F0F10",
            BackgroundGray = "#222226",
            Surface = "#18181B",
            DrawerBackground = "#151517",
            DrawerText = "#A1A1AA",
            DrawerIcon = "#A1A1AA",
            AppbarBackground = "#0F0F10",
            AppbarText = "#F4F1EC",
            TextPrimary = "#F4F1EC",
            TextSecondary = "#A1A1AA",
            TextDisabled = "#6B6B73",
            ActionDefault = "#A1A1AA",
            ActionDisabled = "#4A4A50",
            ActionDisabledBackground = "#222226",
            Divider = "#2E2E33",
            DividerLight = "#222226",
            LinesDefault = "#2E2E33",
            LinesInputs = "#3A3A40",
            TableLines = "#2E2E33",
            TableStriped = "#1C1C1F",
            TableHover = "#222226",
            OverlayDark = "rgba(0,0,0,0.6)"
        },
        PaletteLight = new PaletteLight
        {
            Primary = "#E2551A",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#C77700",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#5C5C66",
            Info = "#C77700",
            Success = "#15803D",
            SuccessContrastText = "#FFFFFF",
            Warning = "#C77700",
            WarningContrastText = "#FFFFFF",
            Error = "#B91C1C",
            Background = "#F3F1ED",
            BackgroundGray = "#F8F6F2",
            Surface = "#FFFFFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#5C5C66",
            DrawerIcon = "#5C5C66",
            AppbarBackground = "#F3F1ED",
            AppbarText = "#18181B",
            TextPrimary = "#18181B",
            TextSecondary = "#5C5C66",
            TextDisabled = "#9A9AA3",
            ActionDefault = "#5C5C66",
            Divider = "#DDD8D0",
            DividerLight = "#E9E5DE",
            LinesDefault = "#DDD8D0",
            LinesInputs = "#C9C3B9",
            TableLines = "#DDD8D0",
            TableStriped = "#F8F6F2",
            TableHover = "#F3F1ED"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
            DrawerWidthLeft = "232px"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = BodyFont, FontWeight = "400" },
            H1 = Heading("3rem", "700"),
            H2 = Heading("2.5rem", "700"),
            H3 = Heading("2rem", "700"),
            H4 = Heading("1.75rem", "700"),
            H5 = Heading("1.5rem", "700"),
            H6 = Heading("1.25rem", "600"),
            Subtitle1 = new Subtitle1Typography { FontFamily = BodyFont, FontWeight = "500" },
            Subtitle2 = new Subtitle2Typography { FontFamily = BodyFont, FontWeight = "500" },
            Body1 = new Body1Typography { FontFamily = BodyFont },
            Body2 = new Body2Typography { FontFamily = BodyFont },
            Button = new ButtonTypography
            {
                FontFamily = HeadingFont,
                FontWeight = "700",
                FontSize = "1rem",
                LetterSpacing = ".04em",
                TextTransform = "uppercase"
            },
            Caption = new CaptionTypography { FontFamily = BodyFont },
            Overline = new OverlineTypography { FontFamily = HeadingFont, FontWeight = "600", LetterSpacing = ".08em" }
        }
    };

    private static H1Typography Heading(string size, string weight)
    {
        return new H1Typography
        {
            FontFamily = HeadingFont,
            FontSize = size,
            FontWeight = weight,
            LineHeight = "1.1",
            LetterSpacing = ".04em",
            TextTransform = "uppercase"
        };
    }
}
