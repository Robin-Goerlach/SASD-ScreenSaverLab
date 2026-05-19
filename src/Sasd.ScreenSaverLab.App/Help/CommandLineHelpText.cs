using System;
using System.IO;
using System.Windows.Forms;

namespace Sasd.ScreenSaverLab.App;

/// <summary>
/// Provides a compact manpage-like command-line reference for the application.
/// </summary>
/// <remarks>
/// The application is a Windows Forms executable, so command-line help cannot rely on
/// a visible console in every launch scenario. For developer runs the text is also
/// written to <see cref="Console.Out" />; for normal Windows runs it is shown in a
/// message box.
/// </remarks>
public static class CommandLineHelpText
{
    /// <summary>
    /// Displays the help text and also writes it to standard output when available.
    /// </summary>
    public static void Show()
    {
        string helpText = Build();

        try
        {
            Console.WriteLine(helpText);
        }
        catch (IOException)
        {
            // GUI applications do not always have a usable stdout stream. The message
            // box below remains the reliable fallback for Windows users.
        }

        MessageBox.Show(
            helpText,
            "SASD-SCREENSAVERLAB(1)",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    /// <summary>
    /// Builds the manpage-like help text.
    /// </summary>
    public static string Build()
    {
        return """
SASD-SCREENSAVERLAB(1)        User Commands        SASD-SCREENSAVERLAB(1)

NAME
    Sasd.ScreenSaverLab.App - experimental screensaver and visualizer host

SYNOPSIS
    Sasd.ScreenSaverLab.App.exe [OPTIONS]

DESCRIPTION
    SASD ScreenSaver Lab is an experimental C#/.NET screensaver and
    visualizer playground for Windows.

    It provides a modular fullscreen host for original animated effects,
    currently Star Drift and Data Stream.

OPTIONS
    /help, /?, -h, --help
        Show this help text and exit.

    /mouse
        Start on the screen where the mouse cursor is currently located.

    /screen:<index>
        Start on a specific screen. Screen indices are zero-based and use
        the order returned by Windows Forms Screen.AllScreens.

        Example:
            /screen:1

    /primary
        Start on the Windows primary screen.

    /all-screens
        Start one fullscreen screensaver window on each connected screen.

    /effect:<name>
        Select the screensaver effect.

        Available effects:
            star-drift
            data-stream

    /star
        Shortcut for /effect:star-drift.

    /stream, /data
        Shortcuts for /effect:data-stream.

    /clock, /clock:on, /clock:true, /show-clock
        Show the clock/date/effect-name overlay. This is the default.

    /no-clock, /clock:off, /clock:false, /hide-clock
        Hide the clock/date/effect-name overlay.

POWER MANAGEMENT
    /allow-sleep
        Do not prevent Windows from entering sleep mode or turning off the
        display. This is the default and respects the active Windows power plan.

    /keep-awake
        Request that Windows keeps the system awake while the screensaver is
        running. The display may still turn off depending on the power plan.

    /keep-display-awake
        Request that Windows keeps both the system and the display awake while
        the screensaver is running. Useful for demos, dashboards, showrooms and
        kiosk-like display scenarios.

EFFECTS
    star-drift
        Calm particle field with subtle depth impression.

    data-stream
        Falling technical glyph streams with an original SASD data-flow look.

EXAMPLES
    Sasd.ScreenSaverLab.App.exe /star /clock

    Sasd.ScreenSaverLab.App.exe /stream /no-clock

    Sasd.ScreenSaverLab.App.exe /all-screens /effect:data-stream

    Sasd.ScreenSaverLab.App.exe /screen:1 /effect:star-drift /clock

    Sasd.ScreenSaverLab.App.exe /stream /no-clock /keep-display-awake

NOTES
    This project is currently a prototype. It starts as a normal Windows
    application and will later support proper .scr screensaver behavior.

VERSION
    0.2.2

AUTHOR
    SASD - Scientific and Software Development
""";
    }
}
