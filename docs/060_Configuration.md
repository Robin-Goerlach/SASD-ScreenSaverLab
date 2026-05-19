# SASD ScreenSaver Lab — Configuration Notes

## 1. Current configuration level

V0.4.5 intentionally still uses command-line configuration for effect selection and now also reads `config/feeds.json` for Amber Feed RSS sources and display timing. This keeps the prototype small and avoids building a settings dialog before the screensaver host and effects are stable.

## 2. Command-line help

The application provides a compact manpage-like help output:

```text
/help
/?
-h
--help
/man
/usage
```

Example:

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /help
```

Because the application is currently a Windows Forms executable, help is written to the console when available and also shown in a Windows message box.

## 3. Power management

The default behavior is:

```text
/allow-sleep
```

This means the application does not prevent Windows from entering sleep mode or turning off the display according to the active power plan.

Optional keep-awake arguments:

```text
/keep-awake
/keep-system-awake
/stay-awake
/no-sleep

/keep-display-awake
/keep-screen-awake
/display-awake
/screen-awake
```

Examples:

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /stream /keep-awake

dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /stream /no-clock /keep-display-awake
```

The implementation uses Windows `SetThreadExecutionState` and deliberately avoids simulated mouse movement.

## 4. Effect selection

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

/effect:data-stream
/effect=data-stream
/effect:datastream
/effect=datastream
/stream
/data
/datastream
/data-stream
/cipherfall

/effect:light-trails
/effect=light-trails
/effect:light-trail
/effect=light-trail
/light
/lights
/trail
/trails
/lighttrail
/light-trails
```

Examples:

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /effect:star-drift

dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /effect:data-stream

dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /effect:light-trails

dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /all-screens /stream /no-clock

dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /light /no-clock
```

Unknown effect names currently fall back to `star-drift` instead of failing at startup.

## 5. Clock overlay

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

dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /all-screens /effect:data-stream /no-clock
```

## 6. Later settings dialog

A later graphical settings dialog should reuse the same concepts instead of introducing a second configuration path. A likely next step would be a small configuration model such as:

```csharp
public sealed record ScreenSaverSettings(
    string EffectName,
    bool ShowClockOverlay,
    string MonitorMode,
    string PowerManagementMode);
```

The settings could later be stored as JSON in the user's application data directory.


## Amber Feed configuration

V0.4.5 reads and validates the Amber Feed configuration file:

```text
config/feeds.json
```

The file defines RSS/Atom source metadata, the Amber Feed reading speed and the adaptive page fill behavior. The current version refreshes enabled RSS/Atom feeds in the background and falls back to cached or demo items when live retrieval fails.

Example keys:

```json
{
  "refreshMinutes": 15,
  "maxItemsPerFeed": 10,
  "requestTimeoutSeconds": 5,
  "useDemoItemsWhenOffline": true,
  "autoFillPage": true,
  "itemsPerPage": 5,
  "minItemsPerPage": 3,
  "maxItemsPerPageOnScreen": 8,
  "pageDurationSeconds": 28,
  "characterRevealRate": 28,
  "theme": "amber",
  "feeds": [
    {
      "name": "Heise Security",
      "url": "https://www.heise.de/security/rss/news-atom.xml",
      "enabled": true
    }
  ]
}
```


Amber Feed display keys:

```text
autoFillPage                Let the effect calculate page density from monitor height.
itemsPerPage                Fixed page size used when autoFillPage is false.
minItemsPerPage             Lower bound for automatic page filling.
maxItemsPerPageOnScreen     Upper bound for automatic page filling.
pageDurationSeconds         Seconds before the next page is displayed.
characterRevealRate         Characters revealed per second by the typewriter animation.
```

For a calmer, more readable display, increase `pageDurationSeconds` and/or decrease `characterRevealRate`. To use available screen space more aggressively, keep `autoFillPage` enabled and raise `maxItemsPerPageOnScreen`.

Amber Feed command-line aliases:

```text
/effect:amber-feed
/effect=amber-feed
/amber
/feed
/retro-feed
```

Feed configuration path aliases:

```text
/feeds:<path>
/feeds=<path>
/feed-config:<path>
/feed-config=<path>
/rss-config:<path>
/rss-config=<path>
/rss:<path>
/rss=<path>
```

Useful test commands:

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /amber /no-clock

dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /amber /feeds:config/feeds.json /no-clock
```
