namespace ItemSpawnerEnhanced.Core;

internal enum UiPaletteToken
{
    Surface,
    SurfaceHover,
    SurfacePressed,
    Accent,
    AccentHover,
    Panel,
    TextPrimary,
    TextMuted,
    DisabledSurface
}

internal readonly struct ControlVisualState
{
    public ControlVisualState(UiPaletteToken background, UiPaletteToken foreground, bool showFocus)
    {
        Background = background;
        Foreground = foreground;
        ShowFocus = showFocus;
    }

    public UiPaletteToken Background { get; }
    public UiPaletteToken Foreground { get; }
    public bool ShowFocus { get; }
}

internal static class ControllerVisualState
{
    public static ControlVisualState ForTag(
        bool active,
        bool focused,
        bool hovered,
        bool pressed)
    {
        UiPaletteToken background;
        if (pressed)
            background = active ? UiPaletteToken.AccentHover : UiPaletteToken.SurfacePressed;
        else if (active)
            background = hovered ? UiPaletteToken.AccentHover : UiPaletteToken.Accent;
        else
            background = hovered ? UiPaletteToken.SurfaceHover : UiPaletteToken.Surface;

        return new ControlVisualState(
            background,
            active ? UiPaletteToken.Panel : UiPaletteToken.TextPrimary,
            focused);
    }

    public static ControlVisualState ForTargetOption(bool current, bool focused, bool hovered) =>
        new(
            current
                ? UiPaletteToken.Accent
                : hovered ? UiPaletteToken.SurfaceHover : UiPaletteToken.Surface,
            current ? UiPaletteToken.Panel : UiPaletteToken.TextPrimary,
            focused);

    public static ControlVisualState ForItem(
        bool canSpawn,
        bool focused,
        bool hovered,
        bool pressed)
    {
        UiPaletteToken background = canSpawn
            ? pressed
                ? UiPaletteToken.SurfacePressed
                : hovered ? UiPaletteToken.SurfaceHover : UiPaletteToken.Surface
            : hovered ? UiPaletteToken.SurfaceHover : UiPaletteToken.DisabledSurface;
        return new ControlVisualState(
            background,
            canSpawn ? UiPaletteToken.TextPrimary : UiPaletteToken.TextMuted,
            focused);
    }

    public static ControlVisualState ForAction(
        bool enabled,
        bool focused,
        bool hovered,
        bool pressed) =>
        new(
            enabled
                ? pressed
                    ? UiPaletteToken.SurfacePressed
                    : hovered ? UiPaletteToken.SurfaceHover : UiPaletteToken.Surface
                : UiPaletteToken.DisabledSurface,
            enabled ? UiPaletteToken.TextPrimary : UiPaletteToken.TextMuted,
            enabled && focused);
}
