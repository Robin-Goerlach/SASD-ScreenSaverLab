namespace Sasd.ScreenSaverLab.Core;

/// <summary>
/// Describes how the application should interact with the Windows power manager
/// while the screensaver host is running.
/// </summary>
/// <remarks>
/// The default is intentionally conservative: SASD ScreenSaver Lab should respect
/// the user's Windows power plan unless the user explicitly requests a keep-awake
/// mode from the command line.
/// </remarks>
public enum PowerManagementMode
{
    /// <summary>
    /// Do not request any keep-awake behavior. Windows may enter sleep or turn off
    /// the display according to the active power plan.
    /// </summary>
    AllowSleep = 0,

    /// <summary>
    /// Request that Windows keeps the system awake while the application runs.
    /// The display may still turn off depending on the active power plan.
    /// </summary>
    KeepSystemAwake = 1,

    /// <summary>
    /// Request that Windows keeps both the system and the display awake while the
    /// application runs.
    /// </summary>
    KeepSystemAndDisplayAwake = 2
}
