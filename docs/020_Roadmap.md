# SASD ScreenSaver Lab — Roadmap

## Guiding idea

The project should grow in small visible increments. Every version should add either a visible effect, a useful host capability, or a necessary Windows screensaver feature.

## Completed milestones

### V0.1 — Initial Star Drift prototype

Implemented:

- Windows Forms fullscreen host
- modular `IScreenSaverEffect` interface
- `StarDriftEffect`
- basic render loop
- clock/date/effect-name overlay
- exit by keyboard, mouse click, or noticeable mouse movement

### V0.1.1 — Multi-monitor startup correction

Implemented:

- explicit monitor bounds instead of blind maximize
- monitor under mouse pointer by default
- `/screen:N`, `/primary`, `/mouse` and `/all-screens`
- one host window per monitor for all-screen mode

### V0.1.2 — Repository-ready polish

Implemented:

- screenshot path for GitHub README
- GitHub Actions build workflow
- repository setup documentation

### V0.1.3 — Configurable clock overlay

Implemented:

- `/clock`, `/no-clock`, `/clock:on`, `/clock:off`
- `ShowClockOverlay` startup option
- configuration documentation

### V0.2 — Data Stream and built-in effect selection

Implemented:

- first data-flow glyph effect, now named `DataStreamEffect`
- `BuiltInScreenSaverEffects` factory
- `EffectName` startup option
- `/effect:star-drift`, `/effect:data-stream`, `/stream` and related aliases

### V0.2.1 — Manpage-like help output and naming cleanup

Implemented:

- `/help`, `/?`, `-h`, `--help`, `/man` and `/usage`
- manpage-like command-line help text
- `docs/070_Command_Line_Reference.md`
- renamed the second effect concept to Data Stream to avoid misleading expectations and avoid close imitation of known film visuals

## Next recommended versions

### V0.2.2 — Small effect cleanup

Possible improvements:

- tune Data Stream speed and density after testing on a real monitor
- add a screenshot for Data Stream
- add parser tests once a test project exists

### V0.3 — Light Trails effect

Add a second more visual effect with glowing curves or moving light bands.

The goal is to move beyond particle dots and text glyphs into a more visual screensaver style.

### V0.4 — Basic configuration dialog

Add a simple Windows Forms dialog for:

- selecting the built-in effect
- showing/hiding the clock overlay
- choosing monitor behavior

The dialog does not need to be pretty at first. It should map to the existing options instead of inventing a second configuration model.

### V0.5 — Real Windows `.scr` packaging

Prepare the application so it can be copied or built as a Windows screensaver file.

Important topics:

- `/s` fullscreen mode
- `/c` configuration mode
- `/p HWND` preview mode
- installer or manual installation notes

### V1.0 — Small stable screensaver collection

A credible first release should include:

- 3 to 5 built-in effects
- stable multi-monitor behavior
- configuration dialog
- real screensaver packaging notes
- clean README screenshots
- basic automated build

## Deferred ideas

The following ideas are intentionally deferred:

- external plugin loading
- audio-reactive visualizer mode
- advanced shader rendering
- SkiaSharp/OpenGL backend
- animated settings preview
- asset-heavy aquarium-like effects

These ideas are interesting, but they should not block a robust and understandable first version.
