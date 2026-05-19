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

### V0.2.2 — Optional power-management modes

Implemented:

- `/allow-sleep` as the explicit default behavior
- `/keep-awake` to request that Windows keeps the system awake
- `/keep-display-awake` to request that Windows keeps both system and display awake
- `PowerManagementMode` startup option
- `PowerKeepAwakeService` wrapper around the Windows execution-state API
- `docs/080_Power_Management.md`

### V0.3 — Light Trails effect

Implemented:

- `LightTrailsEffect` with softly glowing moving trails
- `/effect:light-trails`
- `/light`, `/trails` and related aliases
- command-line help and configuration documentation updated for three effects

## Next recommended versions

### V0.3.1 — Small effect cleanup

Possible improvements:

- tune Data Stream speed and density after testing on a real monitor
- tune Light Trails brightness, count and movement after testing on a real monitor
- add screenshots for Data Stream and Light Trails
- add parser tests once a test project exists

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


### V0.4 — Amber Feed effect

Status: implemented as V0.4.0 prototype.

Goal: add a useful retro terminal-style feed display while keeping network access out of the rendering code for now.

Implemented in V0.4.0:

- `AmberFeedEffect`
- amber monochrome terminal look
- scanlines, glow, typewriter reveal, page rotation and progress bar
- demo feed items
- command-line aliases `/effect:amber-feed`, `/amber` and `/feed`
- initial `config/feeds.json` example file
- dedicated Amber Feed design note

Deferred:

- reading the JSON configuration file
- RSS/Atom download service
- cache and timeout handling
- configurable terminal themes

### V0.4.1 — Feed configuration loading

Planned next step:

- parse `config/feeds.json`
- validate feed entries
- expose feed settings to the application layer
- keep demo mode as safe fallback

### V0.4.2 — RSS/Atom retrieval and cache

Planned next step:

- download feeds with timeouts
- convert RSS/Atom entries into internal feed items
- cache last successful result
- keep rendering cached or demo items when offline
