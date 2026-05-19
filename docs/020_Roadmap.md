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

### V0.4.0 — Amber Feed visual prototype

Implemented:

- `AmberFeedEffect`
- amber monochrome terminal look
- scanlines, glow, typewriter reveal, page rotation and progress bar
- demo feed items
- command-line aliases `/effect:amber-feed`, `/amber` and `/feed`
- initial `config/feeds.json` example file
- dedicated Amber Feed design note

### V0.4.1 — Feed configuration loading

Implemented:

- parse and validate `config/feeds.json`
- accept custom feed configuration path with `/feeds:<path>`
- show configured feed sources as Amber Feed terminal preview items
- keep demo mode as safe fallback when no usable configuration is available
- avoid live network I/O in the visual effect

### V0.4.2 — RSS/Atom retrieval and cache

Implemented:

- download enabled RSS/Atom feeds with short timeouts
- convert RSS `item` and Atom `entry` elements into display items
- clean HTML fragments and normalize feed text
- cache the last successful result in local app data
- keep rendering cached or demo items when live retrieval fails
- keep network access off the UI thread by refreshing in the background

### V0.4.3 — Configurable Amber Feed reading speed

Implemented:

- configurable `itemsPerPage` in `config/feeds.json`
- configurable `pageDurationSeconds` in `config/feeds.json`
- configurable `characterRevealRate` in `config/feeds.json`
- calmer default Amber Feed timing so pages stay readable longer
- runtime clamping for unreasonable timing values
- Amber Feed documentation updated with slow/calm/fast examples

### V0.4.4 — Amber Feed layout hardening

Implemented:

- clipped single-line rendering for long header, status and source text
- bounded wrapped rendering for long feed titles and summaries
- ellipsis trimming for overly long RSS text
- vertical content boundary so feed text cannot overwrite the progress bar or footer
- improved resilience for unusually long URLs, titles or descriptions in external feeds

## Next recommended versions

### V0.4.5 — Adaptive Amber Feed page fill

Implemented:

- optional automatic page filling based on the current monitor/window height
- new `autoFillPage` configuration switch
- new `minItemsPerPage` and `maxItemsPerPageOnScreen` configuration bounds
- safer fallback to fixed `itemsPerPage` when automatic filling is disabled
- better use of large screens without making small screens too dense

### V0.4.6 — Amber Feed refinement

Possible improvements:

- recurring timed refresh loop while the screensaver stays open for a long time
- better status messages for failed feeds
- optional maximum total item count
- screenshots for Amber Feed with real feed content
- small feed parser tests once a test project exists

### V0.5 — Basic configuration dialog

Add a simple Windows Forms dialog for:

- selecting the built-in effect
- showing/hiding the clock overlay
- choosing monitor behavior
- selecting or editing the Amber Feed configuration path

The dialog does not need to be pretty at first. It should map to the existing options instead of inventing a second configuration model.

### V0.6 — Real Windows `.scr` packaging

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
- configurable terminal themes beyond the current amber palette

These ideas are interesting, but they should not block a robust and understandable first version.
