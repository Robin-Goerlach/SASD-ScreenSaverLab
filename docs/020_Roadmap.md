# SASD ScreenSaver Lab — Roadmap

## V0.1 — Technical Shell and Star Drift

Goal: create the smallest useful version.

Scope:

- Solution and project structure
- Windows Forms fullscreen host
- Basic render loop
- `IScreenSaverEffect` interface
- `StarDriftEffect`
- Clock/date overlay
- Exit behavior for Esc, mouse click, and noticeable mouse movement

Status: initial prototype.

## V0.2 — Digital Rain

Goal: add a second clearly different effect.

Possible scope:

- `DigitalRainEffect`
- Falling numbers, symbols, or SASD-themed glyphs
- Configurable density and speed later
- Effect selection in code first, UI later

## V0.3 — Light Sticks

Goal: create a more iTunes-visualizer-like effect without audio analysis.

Possible scope:

- Moving light bars
- Reflection or pseudo-floor effect
- Smooth color transitions
- Soft glow

## V0.4 — Configuration Basics

Goal: make the prototype easier to use.

Possible scope:

- Simple settings dialog
- Select effect
- Toggle clock overlay
- Set speed factor
- Set particle count
- Store settings in a simple JSON file

## V0.5 — Windows Screensaver Mode

Goal: behave more like a real Windows screensaver.

Possible scope:

- `.scr` packaging research
- `/s` fullscreen argument
- `/c` configuration argument
- `/p <hwnd>` preview mode investigation
- installer or manual setup instructions

## V0.6 — Nebula Cloud

Goal: add a calm space/nebulous effect.

Possible scope:

- Layered particles
- Slow drift
- Color palettes
- Soft cloudy look

## V0.7 — Magnet Field

Goal: create a more advanced showcase effect.

Possible scope:

- Pseudo-3D particle sphere
- Field-line movement
- Rotating camera impression
- Optional audio-reactive mode later

## V1.0 — Small Screensaver Collection

Goal: publish a credible first public version.

Expected content:

- Several stable built-in effects
- Basic configuration
- Windows screensaver mode
- Professional README
- Screenshots or animated GIFs
- Clear license
- Known limitations

## Deferred ideas

The following ideas are intentionally postponed:

- real external plugin system,
- shader-based rendering,
- OpenGL/DirectX backend,
- audio analysis,
- multi-monitor support beyond basic fullscreen,
- effect marketplace or module installer,
- advanced preview host.
