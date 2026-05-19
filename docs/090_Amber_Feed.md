# Amber Feed Effect

## Purpose

`AmberFeedEffect` is the first prototype of a useful information screensaver. It presents rotating feed-style messages on a dark amber retro terminal screen inspired by 1980s monochrome monitors and teletext-like information pages.

V0.4.1 keeps the strong retro look from V0.4.0 and adds the first real configuration step: the effect can now load and validate RSS/Atom source definitions from `config/feeds.json`.

The application still does **not** download live RSS/Atom items. This is intentional. Configuration loading, live network retrieval, caching and timeout handling are separate concerns and should be implemented in small, safe steps.

## Current status in V0.4.1

Implemented:

- amber monochrome terminal frame
- dark CRT-like background
- scanline overlay
- typewriter-style text reveal
- rotating terminal pages
- page progress bar
- blinking cursor
- command-line selection via `/effect:amber-feed`, `/amber` or `/feed`
- loading and validating `config/feeds.json`
- previewing configured RSS/Atom sources as terminal status items
- fallback demo messages when no usable configuration is available
- custom feed configuration path via `/feeds:<path>`

Not implemented yet:

- downloading real RSS/Atom feed items
- feed cache
- network timeout handling
- configurable themes beyond the current amber palette
- per-feed filtering or prioritization

## Configuration file

Default location:

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
    },
    {
      "name": "Tagesschau",
      "url": "https://www.tagesschau.de/xml/rss2/",
      "enabled": true
    }
  ]
}
```

V0.4.1 validates enabled feed URLs and accepts only absolute `http` or `https` URLs. Disabled feeds stay in the configuration file but are not shown in the terminal preview.

## Command-line options

Use the default configuration file:

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /amber /no-clock
```

Use an explicit configuration file:

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /amber /feeds:config/feeds.json /no-clock
```

Supported aliases:

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

## Architecture

The effect itself still does not download feeds. V0.4.1 adds only the configuration boundary:

```text
AmberFeedEffect
  Renders terminal visuals and feed-style display items.

AmberFeedConfigurationLoader
  Reads and validates config/feeds.json.
  Converts configured sources into preview display items.

Future RssFeedService
  Downloads RSS/Atom feeds with timeout handling.

Future FeedCache
  Stores recent items so the screensaver remains usable offline.
```

This avoids a frozen or black screensaver when the network is unavailable or a feed endpoint is slow.

## Design notes

The effect deliberately avoids names and assets from commercial film, operating-system or screensaver products. It uses generic retro-terminal and teletext ideas as inspiration and implements an original SASD-style visual presentation.
