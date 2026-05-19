# Amber Feed Effect

## Purpose

`AmberFeedEffect` is the first prototype of a useful information screensaver. It presents rotating feed-style messages on a dark amber retro terminal screen inspired by 1980s monochrome monitors and teletext-like information pages.

V0.4.5 keeps the strong retro look, loads RSS/Atom source definitions from `config/feeds.json`, refreshes live feed items in the background and falls back to cached or demo messages when feeds are unavailable.

## Current status in V0.4.5

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
- timeout-safe background RSS/Atom retrieval
- basic RSS `item` parsing
- basic Atom `entry` parsing
- local cache fallback
- demo fallback messages when no usable configuration, cache or live feed is available
- custom feed configuration path via `/feeds:<path>`
- configurable terminal timing via `itemsPerPage`, `pageDurationSeconds` and `characterRevealRate`
- adaptive page filling through `autoFillPage`, `minItemsPerPage` and `maxItemsPerPageOnScreen`
- bounded text rendering with wrapping, clipping and ellipsis trimming for unusually long feed text

Still intentionally simple:

- no graphical feed editor yet
- no per-feed priority, include/exclude keywords or categories yet
- no recurring timed refresh loop yet; V0.4.5 performs an initial background refresh
- no configurable themes beyond the current amber palette

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
    },
    {
      "name": "Tagesschau",
      "url": "https://www.tagesschau.de/xml/rss2/",
      "enabled": true
    }
  ]
}
```

V0.4.5 validates enabled feed URLs and accepts only absolute `http` or `https` URLs. Disabled feeds stay in the configuration file but are not retrieved.

## Reading speed

Amber Feed is intended to be readable from a distance, not just visually busy. V0.4.5 therefore reads display timing and page density from `config/feeds.json`:

```json
{
  "autoFillPage": true,
  "itemsPerPage": 5,
  "minItemsPerPage": 3,
  "maxItemsPerPageOnScreen": 8,
  "pageDurationSeconds": 28,
  "characterRevealRate": 28
}
```

Meaning:

```text
autoFillPage                Whether the effect should calculate page density from monitor height.
itemsPerPage                Fixed page size used when autoFillPage is false.
minItemsPerPage             Lower bound for automatic page filling.
maxItemsPerPageOnScreen     Upper bound for automatic page filling.
pageDurationSeconds         How long one page stays visible before switching.
characterRevealRate         How many characters are revealed per second.
```

Good starting points:

```text
Adaptive calm:   autoFillPage=true, minItemsPerPage=3, maxItemsPerPageOnScreen=8, pageDurationSeconds=28
Very slow:       autoFillPage=false, itemsPerPage=2, pageDurationSeconds=40, characterRevealRate=18
Faster demo:     autoFillPage=false, itemsPerPage=4, pageDurationSeconds=16, characterRevealRate=48
```

The effect clamps unreasonable values at runtime, so an accidental extreme value should not crash rendering.

## Cache

Live feed items are stored in a small best-effort cache so the screensaver can still show useful information when the network is unavailable.

Default cache path on Windows:

```text
%LOCALAPPDATA%\SASD\ScreenSaverLab\amber-feed-cache.json
```

The cache is not security-sensitive and should not contain credentials. It only stores already-public feed titles and short summaries.

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

V0.4.5 keeps rendering, configuration loading, network retrieval and caching separate:

```text
AmberFeedEffect
  Renders terminal visuals.
  Starts one background refresh after initialization.
  Swaps displayed items when live, cached or fallback data becomes available.

AmberFeedConfigurationLoader
  Reads and validates config/feeds.json.
  Returns validated enabled HTTP/HTTPS feed sources.

AmberFeedRssLoader
  Downloads RSS/Atom feeds with short timeout handling.
  Converts feed XML into display items.

AmberFeedXmlParser
  Parses conservative RSS item and Atom entry structures.
  Cleans HTML snippets and normalizes whitespace.

AmberFeedCacheService
  Stores and loads recent display items from local app data.
```

This avoids a frozen or black screensaver when the network is unavailable or a feed endpoint is slow.

## Design notes

The effect deliberately avoids names and assets from commercial film, operating-system or screensaver products. It uses generic retro-terminal and teletext ideas as inspiration and implements an original SASD-style visual presentation.
