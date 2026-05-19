# Amber Feed Effect

## Purpose

`AmberFeedEffect` is the first prototype of a useful information screensaver. It presents rotating feed-style messages on a dark amber retro terminal screen inspired by 1980s monochrome monitors and teletext-like information pages.

V0.4.0 focuses on the visual effect and uses built-in demo messages. Real RSS loading, caching and error handling are intentionally deferred to keep the rendering step stable and easy to test.

## Current status in V0.4.0

Implemented:

- amber monochrome terminal frame
- dark CRT-like background
- scanline overlay
- typewriter-style text reveal
- rotating demo feed pages
- page progress bar
- blinking cursor
- command-line selection via `/effect:amber-feed`, `/amber` or `/feed`
- initial example configuration file at `config/feeds.json`

Not implemented yet:

- reading `config/feeds.json`
- downloading real RSS/Atom feeds
- feed cache
- network timeout handling
- configurable themes
- per-feed filtering or prioritization

## Planned configuration file

The RSS sources should be stored in a configuration file instead of being hard-coded in C# code.

Planned default location:

```text
config/feeds.json
```

Example:

```json
{
  "refreshMinutes": 15,
  "maxItemsPerFeed": 10,
  "useDemoItemsWhenOffline": true,
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

## Planned architecture

The effect itself should not download feeds directly. It should only render already prepared feed items.

Planned split:

```text
AmberFeedEffect
  Renders feed items and terminal visuals.

FeedConfigurationLoader
  Reads config/feeds.json.

RssFeedService
  Downloads RSS/Atom feeds with timeout handling.

FeedCache
  Stores recent items so the screensaver remains usable offline.
```

This avoids a frozen or black screensaver when the network is unavailable or a feed endpoint is slow.

## Useful test commands

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /amber /no-clock
```

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /effect:amber-feed /keep-display-awake
```

## Design notes

The effect deliberately avoids names and assets from commercial film, operating-system or screensaver products. It uses generic retro-terminal and teletext ideas as inspiration and implements an original SASD-style visual presentation.
