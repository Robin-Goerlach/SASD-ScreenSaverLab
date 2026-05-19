namespace Sasd.ScreenSaverLab.Core;

/// <summary>
/// Represents command-line startup options passed by Windows or by the developer.
/// </summary>
/// <param name="Mode">The requested application mode.</param>
/// <param name="PreviewWindowHandle">The native preview window handle, if Windows supplied one.</param>
/// <param name="TargetScreenIndex">
/// Optional zero-based screen index. The value follows the order returned by
/// <c>Screen.AllScreens</c> in the Windows Forms application project.
/// </param>
/// <param name="UseMouseScreen">
/// When true, the application starts on the monitor that currently contains the mouse pointer.
/// This is convenient during development on a multi-monitor workstation.
/// </param>
/// <param name="UseAllScreens">
/// When true, the application opens one fullscreen host window per connected monitor.
/// </param>
/// <param name="UsePrimaryScreen">
/// When true, the application explicitly starts on the Windows primary monitor.
/// </param>
/// <param name="ShowClockOverlay">
/// When true, the host draws the built-in clock/date/effect-name overlay. When false,
/// only the visual effect itself is rendered.
/// </param>
/// <param name="EffectName">
/// Gets the requested built-in effect name or alias. The application project resolves this
/// value to an effect implementation through the built-in effect factory.
/// </param>
/// <param name="ShowHelp">
/// When true, the application prints/displays the command-line help and exits without
/// starting a screensaver window.
/// </param>
/// <param name="PowerManagementMode">
/// Controls whether the application asks Windows to keep the system and/or display
/// awake while the screensaver host is running. The default respects the active
/// Windows power plan.
/// </param>
public sealed record ScreenSaverStartupOptions(
    ScreenSaverMode Mode,
    nint? PreviewWindowHandle = null,
    int? TargetScreenIndex = null,
    bool UseMouseScreen = true,
    bool UseAllScreens = false,
    bool UsePrimaryScreen = false,
    bool ShowClockOverlay = true,
    string EffectName = "star-drift",
    bool ShowHelp = false,
    PowerManagementMode PowerManagementMode = PowerManagementMode.AllowSleep)
{
    /// <summary>
    /// Gets default options for a normal fullscreen development run.
    /// </summary>
    /// <remarks>
    /// The default intentionally uses the monitor under the mouse pointer instead of
    /// always using the Windows primary monitor. This makes testing on a secondary
    /// display much easier. The clock overlay is enabled by default because it is useful
    /// during the early prototype phase.
    /// </remarks>
    public static ScreenSaverStartupOptions Default => new(ScreenSaverMode.Normal);
}
