# SASD ScreenSaver Lab — Configuration Notes

## 1. Current configuration level

V0.1.3 intentionally uses command-line configuration only. This keeps the prototype small and avoids building a settings dialog before the screensaver host and effects are stable.

## 2. Clock overlay

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
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /all-screens /no-clock
```

## 3. Later settings dialog

A later graphical settings dialog should reuse the same concepts instead of introducing a second configuration path. A likely next step would be a small configuration model such as:

```csharp
public sealed record ScreenSaverSettings(
    string EffectName,
    bool ShowClockOverlay,
    bool UseAllScreens);
```

The settings could later be stored as JSON in the user's application data directory.
