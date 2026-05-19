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

V0.4.1 reads RSS/Atom source definitions from `config/feeds.json` and shows them as preview items. Later versions should download and cache real feed entries while still falling back to cached or demo messages when offline.
