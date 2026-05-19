# SASD ScreenSaver Lab — Power Management

## 1. Goal

SASD ScreenSaver Lab can optionally prevent Windows from putting the computer to sleep while the screensaver host is running.

This is useful for:

- demos,
- dashboards,
- showrooms,
- kiosk-like displays,
- long-running visual tests.

The feature is deliberately opt-in. By default, the application respects the active Windows power plan.

## 2. Why not move the mouse?

Move-Mouse-style applications often simulate mouse activity to keep a PC awake. SASD ScreenSaver Lab does not do that.

Artificial input can be intrusive, can interfere with user activity, and can feel like a workaround. For this project, the cleaner approach is the Windows execution-state API.

## 3. Implementation approach

The application uses the Windows `SetThreadExecutionState` API through a small wrapper class:

```text
src/Sasd.ScreenSaverLab.App/Power/PowerKeepAwakeService.cs
```

The wrapper is activated before the fullscreen screensaver windows are shown and disposed after the application exits. During disposal it clears the continuous execution-state request.

## 4. Modes

### Default mode

```text
/allow-sleep
```

The application does not prevent sleep or display timeout.

### Keep system awake

```text
/keep-awake
```

The application requests that Windows keeps the computer awake while SASD ScreenSaver Lab is running. Depending on the active power plan, the display may still turn off.

### Keep system and display awake

```text
/keep-display-awake
```

The application requests that Windows keeps both the computer and the display awake while SASD ScreenSaver Lab is running.

This is the most useful mode for demos and dashboard-style usage.

## 5. Examples

```powershell
# Respect the Windows power plan. This is the default.
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /allow-sleep
```

```powershell
# Keep the computer awake, but allow the display to follow the power plan.
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /stream /keep-awake
```

```powershell
# Keep both the computer and the display awake.
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /stream /no-clock /keep-display-awake
```

```powershell
# Multi-monitor demo mode.
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /all-screens /stream /keep-display-awake
```

## 6. Design note

The feature should remain optional. A screensaver that silently prevents sleep would be surprising and could waste energy. The user must explicitly request keep-awake behavior through the command line or, later, through a settings dialog.
