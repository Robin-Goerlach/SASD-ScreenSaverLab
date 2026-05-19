# SASD ScreenSaver Lab — V0.6.1 Effect Alias Fix

V0.6.1 corrects the effect-selection behavior for the V0.6 effects. During testing,
`/lab` and `/pulse` could still behave like the default effect when parser and factory
state did not match during project updates.

## Changes

- Centralized built-in effect aliases in `BuiltInScreenSaverEffects`.
- Added a second raw-argument resolution pass in `Program.cs`.
- Added `/list-effects`, `/effects` and `/effect-list`.
- Updated help output and documentation to mention the diagnostic command.

## Test commands

```powershell
dotnet run --project src\Sasd.ScreenSaverLab.App\Sasd.ScreenSaverLab.App.csproj -- /list-effects
dotnet run --project src\Sasd.ScreenSaverLab.App\Sasd.ScreenSaverLab.App.csproj -- /lab /no-clock
dotnet run --project src\Sasd.ScreenSaverLab.App\Sasd.ScreenSaverLab.App.csproj -- /pulse /no-clock
dotnet run --project src\Sasd.ScreenSaverLab.App\Sasd.ScreenSaverLab.App.csproj -- /effect:lab-console /no-clock
dotnet run --project src\Sasd.ScreenSaverLab.App\Sasd.ScreenSaverLab.App.csproj -- /effect:system-pulse /no-clock
```

## Notes

This is a robustness fix, not a visual redesign. The actual visual effects remain
`LabConsoleEffect` and `SystemPulseEffect` from V0.6.0.
