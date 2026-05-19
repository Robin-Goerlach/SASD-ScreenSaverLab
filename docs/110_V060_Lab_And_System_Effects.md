# SASD ScreenSaver Lab — V0.6.0 Lab and System Effects

## Overview

V0.6.0 adds two new built-in effects that move the project closer to the SASD visual identity: scientific software, research tooling and technical infrastructure.

The new effects are:

- `LabConsoleEffect`
- `SystemPulseEffect`

Both effects are implemented as asset-free GDI+ screensaver effects and use the existing `IScreenSaverEffect` interface.

## LabConsoleEffect

`LabConsoleEffect` renders a fictional laboratory and research-data console. It displays generated sample rows, assay types, signal percentages, temperatures, pH values, status markers, signal traces and a small event log.

The effect does not access real laboratory data. All rows and events are generated demonstration data. This keeps V0.6.0 safe, self-contained and suitable for GitHub screenshots or demos.

Supported aliases:

```text
/effect:lab-console
/lab
/console
/research-console
```

Suggested test command:

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /lab /no-clock
```

## SystemPulseEffect

`SystemPulseEffect` renders an abstract local telemetry visualization. It shows CPU, RAM and network activity as pulse rings, metric cards and sparkline histories.

The effect samples lightweight local telemetry where available:

- CPU through the Windows `GetSystemTimes` API,
- memory load through `GlobalMemoryStatusEx`,
- network activity through .NET network-interface counters.

If one of these sources is unavailable, the effect falls back to safe values and continues rendering. It is a screensaver visualization, not a monitoring or alerting replacement.

Supported aliases:

```text
/effect:system-pulse
/pulse
/system
/telemetry
```

Suggested test command:

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /pulse /no-clock
```

## Design decision

Both effects are intentionally built into the existing effects project rather than loaded as plugins. This keeps the current architecture simple while the effect API is still evolving.

A later plugin model can still be introduced once the built-in effect collection has stabilized.
