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
- message boxes for currently unsupported configure/preview modes.

### Core project

The core project owns abstractions and simple shared models:

- `IScreenSaverEffect`,
- `IEffectClock`,
- `SystemEffectClock`,
- `ScreenSaverStartupOptions`,
- `ScreenSaverCommandLineParser`,
- `ScreenSaverMode`.

The core project does not instantiate concrete effects.

### Effects project

The effects project owns visual implementations:

- `StarDriftEffect`,
- `DigitalRainEffect`,
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

V0.2 adds command-line effect selection:

```text
/effect:star-drift
/effect:digital-rain
/rain
/star
```

The parser stores the requested name in `ScreenSaverStartupOptions.EffectName`.

The app then creates one independent effect instance per screen through:

```csharp
BuiltInScreenSaverEffects.Create(options.EffectName)
```

This keeps the parser independent from the concrete effect classes.

## 6. Configuration approach

The current version uses command-line configuration only.

A later graphical configuration dialog should reuse the same concepts:

- effect name,
- clock overlay enabled/disabled,
- monitor behavior.

A future persistent model might look like this:

```csharp
public sealed record ScreenSaverSettings(
    string EffectName,
    bool ShowClockOverlay,
    string MonitorMode);
```

## 7. Deliberately postponed architecture

The following are intentionally postponed:

- external plugin loading,
- dependency injection container,
- complex rendering engine,
- settings persistence,
- installer,
- audio analysis.

The project should first become a small, stable, visually useful screensaver lab.
