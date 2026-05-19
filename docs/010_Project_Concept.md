# SASD ScreenSaver Lab — Project Concept

## 1. Purpose

SASD ScreenSaver Lab is a small software project for creating original fullscreen animation effects that can later be used as Windows screensavers, desktop visualizers, or visual demonstration modules.

The project should be useful as:

- a C# desktop programming exercise,
- a visual SASD showcase,
- a playground for animation and rendering concepts,
- a possible basis for a real Windows `.scr` screensaver collection,
- a future bridge toward audio-reactive visualizers.

## 2. What the project should be

The project should become a small, understandable, extensible screensaver host.

It should provide:

- a fullscreen host window,
- a stable render loop,
- a clean effect interface,
- several visually pleasing built-in effects,
- optional overlays such as clock/date,
- later: configuration and Windows screensaver integration.

## 3. What the project should not be in V0.1

V0.1 should not become a large graphics engine.

The first version should avoid:

- external plugin loading,
- complex 3D engines,
- heavy shader infrastructure,
- audio analysis,
- a large configuration UI,
- exact reimplementation of historic commercial screensavers.

## 4. Legal and design boundary

Historic screensavers and iTunes visualizers are useful inspiration, but the project should use its own names, assets, algorithms, and visual identity.

The project should not copy:

- protected names,
- original graphics,
- original icons,
- recognisable commercial characters,
- exact animation sequences.

Instead, the project should create original SASD-style effects such as `StarDrift`, `DataStream`, `LightSticks`, `NebulaCloud`, or `MagnetField`.

## 5. Initial product idea

The initial product is named:

```text
SASD ScreenSaver Lab
```

The first usable effect is:

```text
Star Drift
```

`Star Drift` shows a calm dark background with drifting particles, light depth impression, and an optional clock overlay.

It is intentionally simple because it forces the basic architecture to work before more complex effects are added.

## 6. Success criteria for V0.1

V0.1 is successful if:

- the application starts reliably,
- a fullscreen animation is visible,
- the animation is smooth enough on a normal PC,
- the app exits cleanly on keyboard or mouse input,
- the effect code is separate from the host code,
- the project can be opened easily in Visual Studio 2022,
- the code is understandable and sufficiently commented.
