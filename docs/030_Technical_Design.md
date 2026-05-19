# SASD ScreenSaver Lab — Technical Design

## 1. Purpose

The technical design should keep the application small enough to understand while still allowing several visual effects to share the same host.

The project is not yet a plugin platform. For now it is a modular Windows Forms application with built-in effects.

## 2. Solution structure

```text
src/
  Sasd.ScreenSaverLab.App/
  Sasd.ScreenSaverLab.Core/
  Sasd.ScreenSaverLab.Effects/
```

### App project

The app project owns Windows Forms startup behavior:

- application entry point,
- screen selection,
- fullscreen host forms,
- Windows mode handling,
- optional Windows power-management keep-awake behavior,
- message boxes for currently unsupported configure/preview modes.

### Core project

The core project owns abstractions and simple shared models:

- `IScreenSaverEffect`,
- `IEffectClock`,
- `SystemEffectClock`,
- `ScreenSaverStartupOptions`,
- `ScreenSaverCommandLineParser`,
- `ScreenSaverMode`,
- `PowerManagementMode`.

The core project does not instantiate concrete effects.

### Effects project

The effects project owns visual implementations:

- `StarDriftEffect`,
- `DataStreamEffect`,
- `LightTrailsEffect`,
- `BuiltInScreenSaverEffects`.

`BuiltInScreenSaverEffects` is a small built-in factory, not an external plugin loader.

## 3. Effect lifecycle

Every effect implements:

```csharp
public interface IScreenSaverEffect
{
    string Name { get; }
    string Description { get; }
    void Initialize(Size viewportSize);
    void Update(TimeSpan elapsed, Size viewportSize);
    void Render(Graphics graphics, Size viewportSize);
}
```

The host form is responsible for the timer and paint cycle. The effect is responsible for its own animation state.

## 4. Multi-monitor behavior

The application uses explicit `Screen.Bounds` placement instead of relying on `WindowState = Maximized`.

This is important because Windows Forms may otherwise maximize a window on the primary monitor even when another monitor was intended.

The current options are:

- monitor under mouse pointer,
- primary monitor,
- zero-based screen index,
- all connected screens.

## 5. Effect selection

V0.2 added command-line effect selection. V0.3 extends the built-in effect list:

```text
/effect:star-drift
/effect:data-stream
/effect:light-trails
/effect:amber-feed
/star
/stream
/light
/trails
/amber
/feed
```

The parser stores the requested name in `ScreenSaverStartupOptions.EffectName`.

The app then creates one independent effect instance per screen through:

```csharp
BuiltInScreenSaverEffects.Create(options.EffectName)
```

This keeps the parser independent from the concrete effect classes.

## 6. Power management

V0.2.2 adds optional keep-awake behavior through the Windows execution-state API.

Supported command-line modes:

```text
/allow-sleep
/keep-awake
/keep-display-awake
```

The default is `/allow-sleep`. This is intentional: a screensaver should not silently override the user's Windows power plan.

The app project owns the platform-specific implementation:

```text
src/Sasd.ScreenSaverLab.App/Power/PowerKeepAwakeService.cs
```

The core project only stores the selected `PowerManagementMode` in `ScreenSaverStartupOptions`.

## 7. Configuration approach

The current version uses command-line configuration only.

A later graphical configuration dialog should reuse the same concepts:

- effect name,
- clock overlay enabled/disabled,
- monitor behavior,
- power-management mode.

A future persistent model might look like this:

```csharp
public sealed record ScreenSaverSettings(
    string EffectName,
    bool ShowClockOverlay,
    string MonitorMode,
    string PowerManagementMode);
```

## 8. Deliberately postponed architecture

The following are intentionally postponed:

- external plugin loading,
- dependency injection container,
- complex rendering engine,
- settings persistence,
- installer,
- audio analysis.

The project should first become a small, stable, visually useful screensaver lab.


## Amber Feed design note

V0.4.1 keeps `AmberFeedEffect` as a non-networked renderer and adds JSON configuration loading for feed source definitions. It intentionally does not download real RSS feeds yet.

The current and planned production split is:

```text
Sasd.ScreenSaverLab.Effects
  AmberFeedEffect
    Draws feed-style items, scanlines, amber terminal frame and animation.

Sasd.ScreenSaverLab.Effects.Feeds
  AmberFeedConfiguration
  AmberFeedConfigurationLoader
  AmberFeedDisplayItem

Future Sasd.ScreenSaverLab.Core or Infrastructure
  FeedItem
  IFeedService
  RssFeedService
  FeedCache
```

The important design rule is that visual effects should not block on network I/O. A future feed service should load feeds asynchronously or ahead of time, apply timeouts, and provide cached items to the effect.
