# SASD ScreenSaver Lab — Command Line Reference

```text
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

NOTES
    This project is currently a prototype. It starts as a normal Windows
    application and will later support proper .scr screensaver behavior.

VERSION
    0.2.1

AUTHOR
    SASD - Scientific and Software Development
```
