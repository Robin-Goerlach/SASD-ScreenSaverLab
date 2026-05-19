# SASD ScreenSaver Lab

![SASD ScreenSaver Lab - Star Drift prototype](docs/screenshots/sasd-screensaverlab-star-drift.png)

**SASD ScreenSaver Lab** is a small experimental Windows desktop project for creating original screensaver and visualizer effects.

The goal is not to copy existing Apple, iTunes, After Dark, or other historic screensavers one-to-one. Instead, this project uses the general idea of animated visual effects as inspiration and builds its own SASD-style effects, names, palettes, and implementation.

## Current status

Version: **V0.1.3 prototype**

Implemented:

- A small Windows Forms host application
- A modular effect interface
- First effect: `StarDriftEffect`
- Fullscreen mode
- Multi-monitor-aware startup
- Basic render loop
- Exit on keyboard, mouse button, or noticeable mouse movement
- Configurable clock overlay via command-line arguments
- Repository-ready README with screenshot
- Initial GitHub Actions build workflow
- Initial project documentation

Not yet implemented:

- Real `.scr` packaging
- Configuration dialog
- Windows preview mode for the screensaver settings panel
- Multiple selectable effects
- Friendly configuration UI for monitor, overlay and effect selection
- Audio-reactive visualizer mode
- Plugin loading from external assemblies

## Intended technology stack

- C#
- .NET 8
- Windows Forms
- Later possibly SkiaSharp, WPF, OpenTK, Silk.NET, or WebView2 depending on the visual direction

## Project structure

```text
SASD-ScreenSaverLab/
  README.md
  LICENSE
  .github/
    workflows/
      dotnet-build.yml
  docs/
    010_Project_Concept.md
    020_Roadmap.md
    030_Technical_Design.md
    040_Effect_Ideas.md
    050_GitHub_Repository_Setup.md
    060_Configuration.md
    screenshots/
      sasd-screensaverlab-star-drift.png
  src/
    Sasd.ScreenSaverLab.App/
    Sasd.ScreenSaverLab.Core/
    Sasd.ScreenSaverLab.Effects/
```

## Build and run

Open `Sasd.ScreenSaverLab.sln` in Visual Studio 2022.

Recommended startup project:

```text
src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj
```

Or run from the command line on Windows with the .NET SDK installed:

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj
```

The application currently starts in fullscreen mode. Press **Esc**, click the mouse, or move the mouse noticeably to close it.

### Multi-monitor startup

V0.1.1 and newer no longer blindly maximize on the Windows primary monitor. In normal development mode the application starts on the monitor that currently contains the mouse pointer.

Useful command-line examples:

```powershell
# Start on the monitor under the mouse pointer.
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /mouse

# Start on screen index 1. Screen indices are zero-based and follow Screen.AllScreens.
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /screen:1

# Start one fullscreen window on every connected monitor.
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /all-screens
```

For the later real Windows screensaver mode (`/s`), the application is prepared to cover all connected monitors.

### Clock overlay

V0.1.3 can show or hide the built-in clock/date/effect-name overlay via command-line arguments. The overlay is enabled by default.

Useful examples:

```powershell
# Start with the clock overlay enabled. This is also the default.
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /clock

# Start without the clock/date/effect-name overlay.
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /no-clock

# Equivalent explicit forms.
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /clock:on
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /clock:off
```

The current implementation keeps this deliberately simple. A graphical settings dialog can later write the same option into a persistent configuration file.

## Design principle

V0.1 deliberately avoids a heavy plugin architecture. Effects are modular inside the solution, but external plugin loading is postponed until there are several real effects and a clearer need for it.

This keeps the first version small, understandable, and robust.

## Repository notes

Suggested repository name:

```text
SASD-ScreenSaverLab
```

Suggested GitHub topics:

```text
csharp dotnet windows-forms screensaver visualizer graphics animation sasd
```

See [`docs/050_GitHub_Repository_Setup.md`](docs/050_GitHub_Repository_Setup.md) for suggested GitHub setup commands.
