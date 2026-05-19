# SASD ScreenSaver Lab — V0.5.0 Effect Expansion

## Purpose

V0.5.0 expands the project from a small prototype collection into a more credible visual screensaver laboratory. The release adds three new built-in effects while keeping the existing simple architecture.

## New effects

### Wireframe Terrain

A retro vector-landscape effect with moving perspective grid lines, low-poly mountain silhouettes, a striped sun and a horizon glow.

Command-line examples:

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /terrain /no-clock
```

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /effect:wireframe-terrain
```

### Plasma Grid

A colorful low-resolution plasma field with a subtle technical grid overlay and slow pulse rings.

Command-line examples:

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /plasma /no-clock
```

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /effect:plasma-grid
```

### Orbit Field

A scientific particle-orbit visualization with guide ellipses, soft field lines and moving attractor points.

Command-line examples:

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /orbit /no-clock
```

```powershell
dotnet run --project src/Sasd.ScreenSaverLab.App/Sasd.ScreenSaverLab.App.csproj -- /effect:orbit-field
```

## Notes

The new effects are intentionally asset-free. They use calculated geometry, particles, simple gradients and GDI+ drawing operations. This keeps the code portable inside the current Windows Forms renderer and avoids introducing OpenGL, SkiaSharp or shader dependencies too early.

A later rendering backend can still be introduced once the effect model is stable enough.


## V0.5.1 - Terrain horizon polish

The `WireframeTerrainEffect` sun is now clipped to the sky area and positioned slightly higher. This prevents the lower part of the sun from being drawn below the horizon line on wide or low-resolution displays.
