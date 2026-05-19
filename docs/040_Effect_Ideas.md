# SASD ScreenSaver Lab — Effect Ideas

## Implemented effects

### Star Drift

A dark, calm star-field with drifting particles and a small clock overlay.

Purpose:

- prove the host/effect architecture,
- provide a pleasant first screensaver,
- keep V0.1 small and readable.

### Data Stream

A SASD data-flow effect with falling technical glyph streams.

Purpose:

- prove that several effects can share the same interface,
- add an effect that looks immediately different from Star Drift,
- keep the implementation readable and independent from external assets.

### Light Trails

A calm visualizer-style effect with glowing moving trails and soft fading.

Purpose:

- introduce a more visual screensaver style beyond particles and glyphs,
- keep the implementation simple enough for GDI+ and Windows Forms,
- create a better base for later light-band, wave and visualizer experiments.

## Recommended next effects

### Nebula Cloud

A soft particle cloud with slow color movement and depth impression.

This would be good after at least one more simpler effect is stable.

### Flying SASD Objects

Small symbolic objects such as servers, disks, code brackets, or SASD cubes flying across the screen.

This could be a playful alternative to historical object-based screensavers, without copying any protected original assets.

### Magnet Field

A more advanced effect with particles orbiting around invisible force fields.

This should wait until the rendering loop and configuration model are stable.

## Deferred asset-heavy effects

### Aquarium-like scene

Possible, but not recommended early.

Reasons:

- needs custom graphics or sprites,
- believable movement is more complex,
- visual quality depends strongly on assets,
- easy to spend time without improving the core architecture.

## Effect naming principles

Effect names should be original and SASD-compatible.

Avoid names that sound like direct copies of commercial screensavers or media franchises.


## Amber Feed

A useful retro-terminal information display. The effect uses a dark amber monochrome look, scanlines, terminal frame, typewriter-style text reveal and rotating feed pages.

V0.4.5 reads RSS/Atom source definitions from `config/feeds.json`, downloads live entries in the background and caches recent items while still falling back to demo messages when offline. V0.4.5 also makes the reading speed configurable and can automatically fill the current monitor height with an appropriate number of feed entries, so the terminal can stay readable while using large screens better.

### Wireframe Terrain

A retro vector landscape with a moving perspective grid, low-poly mountains and a subtle horizon glow.

Purpose:

- add a clearly different 1980s-inspired visual style,
- stay asset-free and GDI+-friendly,
- provide a strong fullscreen effect for demos and screenshots.

### Plasma Grid

A low-resolution plasma field with a soft technical grid overlay.

Purpose:

- bring a classic demo-scene style into the collection,
- test a small internal bitmap buffer that is scaled to the screen,
- create a colorful but still restrained visual effect.

### Orbit Field

A scientific-looking field of orbiting particles, guide ellipses and slow attractor points.

Purpose:

- strengthen the scientific/SASD visual identity,
- provide a calm effect that looks closer to a simulation or laboratory display,
- prepare later ideas such as magnet-field or particle-system effects.

### Lab Console

A fictional laboratory and research-data console with sample IDs, assay types, signal traces, quality/status values and a small event log.

Purpose:

- strengthen the scientific identity of the SASD ScreenSaver Lab,
- provide a visually useful research-console effect without connecting to real lab data,
- prepare later ideas such as configurable local data sources or SASD project status feeds.

### System Pulse

An abstract local system telemetry screensaver with CPU, memory and network activity rendered as pulse rings, metric cards and sparklines.

Purpose:

- add a practical system-themed effect,
- make a screen look alive during demos or workstation idle time,
- create a foundation for later system-monitor dashboard ideas without turning the screensaver into a full monitoring product.
