namespace Sasd.ScreenSaverLab.Core;

/// <summary>
/// Describes the requested startup mode of the application.
/// </summary>
public enum ScreenSaverMode
{
    /// <summary>
    /// Normal interactive run. Useful while developing and debugging.
    /// </summary>
    Normal,

    /// <summary>
    /// Fullscreen screensaver mode, usually requested with /s.
    /// </summary>
    Fullscreen,

    /// <summary>
    /// Configuration mode, usually requested with /c.
    /// </summary>
    Configure,

    /// <summary>
    /// Preview mode inside the Windows screensaver settings dialog, usually requested with /p.
    /// </summary>
    Preview
}
