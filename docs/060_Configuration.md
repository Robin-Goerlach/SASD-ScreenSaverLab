# SASD ScreenSaver Lab — Configuration Notes

## 1. Current configuration level

V0.2 intentionally uses command-line configuration only. This keeps the prototype small and avoids building a settings dialog before the screensaver host and effects are stable.

## 2. Effect selection

The default effect is:

```text
star-drift
```

Supported effect arguments:

```text
/effect:star-drift
/effect=star-drift
/star
/stars
/star-drift

/effect:digital-rain
/effect=digital-rain
/rain
/digital-rain
/code-rain
/matrix
```

Examples:

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /effect:star-drift

dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /effect:digital-rain

dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /all-screens /rain /no-clock
```

Unknown effect names currently fall back to `star-drift` instead of failing at startup.

## 3. Clock overlay

The clock overlay is enabled by default. It currently contains:

- effect name,
- current time,
- current date.

Supported command-line arguments:

```text
/clock
/show-clock
/with-clock
/clock:on
/clock=on
/clock:true
/clock=true

/no-clock
/hide-clock
/without-clock
/clock:off
/clock=off
/clock:false
/clock=false
```

Examples:

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /clock

dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /no-clock

dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /all-screens /effect:digital-rain /no-clock
```

## 4. Later settings dialog

A later graphical settings dialog should reuse the same concepts instead of introducing a second configuration path. A likely next step would be a small configuration model such as:

```csharp
public sealed record ScreenSaverSettings(
    string EffectName,
    bool ShowClockOverlay,
    string MonitorMode);
```

The settings could later be stored as JSON in the user's application data directory.
