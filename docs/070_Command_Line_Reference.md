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
    currently Star Drift, Data Stream, Light Trails and Amber Feed.

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
            light-trails
            amber-feed

    /star
        Shortcut for /effect:star-drift.

    /stream, /data
        Shortcuts for /effect:data-stream.

    /light, /trails
        Shortcuts for /effect:light-trails.

    /amber, /feed
        Shortcuts for /effect:amber-feed.

    /clock, /clock:on, /clock:true, /show-clock
        Show the clock/date/effect-name overlay. This is the default.

    /no-clock, /clock:off, /clock:false, /hide-clock
        Hide the clock/date/effect-name overlay.

    /feeds:<path>
        Select the JSON configuration file used by the Amber Feed effect.
        The default is config/feeds.json.

        Example:
            /feeds:config/feeds.json

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

    light-trails
        Soft glowing light trails moving across a calm visualizer field.

    amber-feed
        Amber retro terminal feed display. V0.4.5 loads and validates
        configured RSS/Atom source definitions from config/feeds.json,
        refreshes live items in the background and falls back to cache or demo items.

EXAMPLES
    Sasd.ScreenSaverLab.App.exe /star /clock

    Sasd.ScreenSaverLab.App.exe /stream /no-clock

    Sasd.ScreenSaverLab.App.exe /all-screens /effect:data-stream

    Sasd.ScreenSaverLab.App.exe /light /no-clock

    Sasd.ScreenSaverLab.App.exe /amber /no-clock

    Sasd.ScreenSaverLab.App.exe /amber /feeds:config/feeds.json /no-clock

    Sasd.ScreenSaverLab.App.exe /screen:1 /effect:star-drift /clock

    Sasd.ScreenSaverLab.App.exe /stream /no-clock /keep-display-awake

NOTES
    This project is currently a prototype. It starts as a normal Windows
    application and will later support proper .scr screensaver behavior.

VERSION
    0.4.5

AUTHOR
    SASD - Scientific and Software Development
```
