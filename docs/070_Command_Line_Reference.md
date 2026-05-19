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
    currently Star Drift, Data Stream, Light Trails, Amber Feed, Wireframe Terrain, Plasma Grid, Orbit Field, Lab Console and System Pulse.

OPTIONS
    /help, /?, -h, --help
        Show this help text and exit.

    /list-effects, /effects, /effect-list
        Show all built-in effect names and aliases, then exit.

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
            wireframe-terrain
            plasma-grid
            orbit-field
            lab-console
            system-pulse

    /star
        Shortcut for /effect:star-drift.

    /stream, /data
        Shortcuts for /effect:data-stream.

    /light, /trails
        Shortcuts for /effect:light-trails.

    /amber, /feed
        Shortcuts for /effect:amber-feed.

    /wire, /terrain
        Shortcuts for /effect:wireframe-terrain.

    /plasma, /grid
        Shortcuts for /effect:plasma-grid.

    /orbit, /field
        Shortcuts for /effect:orbit-field.

    /lab, /console
        Shortcuts for /effect:lab-console.

    /pulse, /system
        Shortcuts for /effect:system-pulse.

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
        Amber retro terminal feed display. The effect loads configured RSS/Atom
        sources, refreshes them in the background and falls back to cache or
        demo items when sources are unavailable. Reading speed is configured
        in config/feeds.json.

    wireframe-terrain
        1980s-inspired vector landscape with moving perspective grid lines.

    plasma-grid
        Colorful retro plasma field with a restrained technical grid overlay.

    orbit-field
        Scientific particle-orbit effect with soft attractor field lines.

    lab-console
        Fictional laboratory and research-data console with sample rows,
        signal traces and event messages. Uses generated demonstration data.

    system-pulse
        Abstract local CPU, RAM and network telemetry visualization with
        pulse rings, metric cards and sparkline histories.

EXAMPLES
    Sasd.ScreenSaverLab.App.exe /star /clock

    Sasd.ScreenSaverLab.App.exe /stream /no-clock

    Sasd.ScreenSaverLab.App.exe /all-screens /effect:data-stream

    Sasd.ScreenSaverLab.App.exe /light /no-clock

    Sasd.ScreenSaverLab.App.exe /amber /no-clock

    Sasd.ScreenSaverLab.App.exe /amber /feeds:config/feeds.json /no-clock

    Sasd.ScreenSaverLab.App.exe /terrain /no-clock

    Sasd.ScreenSaverLab.App.exe /plasma /no-clock

    Sasd.ScreenSaverLab.App.exe /orbit /no-clock

    Sasd.ScreenSaverLab.App.exe /lab /no-clock

    Sasd.ScreenSaverLab.App.exe /pulse /no-clock

    Sasd.ScreenSaverLab.App.exe /screen:1 /effect:star-drift /clock

    Sasd.ScreenSaverLab.App.exe /stream /no-clock /keep-display-awake

NOTES
    This project is currently a prototype. It starts as a normal Windows
    application and will later support proper .scr screensaver behavior.

VERSION
    0.6.1

AUTHOR
    SASD - Scientific and Software Development
```
