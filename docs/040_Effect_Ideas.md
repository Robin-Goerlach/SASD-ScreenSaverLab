# SASD ScreenSaver Lab — Effect Ideas

## Implemented effects

### Star Drift

A dark, calm star-field with drifting particles and a small clock overlay.

Purpose:

- prove the host/effect architecture,
- provide a pleasant first screensaver,
- keep V0.1 small and readable.

### Digital Rain

A SASD code-rain effect with falling technical glyph streams.

Purpose:

- prove that several effects can share the same interface,
- add an effect that looks immediately different from Star Drift,
- keep the implementation readable and independent from external assets.

## Recommended next effects

### Light Trails

Moving glowing lines or bands with soft fading.

This would be a good V0.3 because it introduces a more visual, flowing screensaver style without requiring real 3D or shaders.

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
