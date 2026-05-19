# SASD ScreenSaver Lab — Technical Design

## 1. Architecture overview

The first version uses a deliberately simple architecture.

```text
Sasd.ScreenSaverLab.App
  Windows Forms host application
  Fullscreen window per selected monitor
  Multi-monitor screen selection
  Render loop
  Input handling
  Clock overlay

Sasd.ScreenSaverLab.Core
  Shared interfaces and runtime helpers
  Effect contract
  Command-line parsing

Sasd.ScreenSaverLab.Effects
  Built-in visual effects
  StarDriftEffect
```

## 2. Main responsibility split

### App project

The app project owns the Windows-specific behavior:

- create one fullscreen form for the selected monitor or monitors,
- place fullscreen forms explicitly on `Screen.Bounds`,
- hide the cursor,
- react to keyboard and mouse input,
- run a timer-based animation loop,
- call the currently selected effect,
- draw optional overlays.

The app project should not contain the actual animation logic of each effect.

### Core project

The core project defines the shared contract.

The most important interface is:

```csharp
public interface IScreenSaverEffect
{
    string Name { get; }
    string Description { get; }
    void Initialize(Size viewportSize);
    void Update(TimeSpan elapsed, Size viewportSize);
    void Render(Graphics graphics, Size viewportSize);
}
```

This is intentionally small. It is easy to understand and sufficient for early effects.

### Effects project

The effects project contains built-in visual effects.

Each effect should:

- implement `IScreenSaverEffect`,
- own its own animation state,
- avoid touching the host form directly,
- be understandable as a separate module.

## 3. Render loop

The host uses a timer to invalidate the form regularly.

On each tick:

1. measure elapsed time,
2. update the active effect,
3. request repaint.

During repaint:

1. clear the background,
2. render the active effect,
3. draw optional overlay elements.

This is not a high-end game loop, but it is sufficient for V0.1 and easy to understand.

## 4. Why Windows Forms first?

Windows Forms is a pragmatic choice for V0.1 because:

- it is simple,
- it works well in Visual Studio,
- it is sufficient for 2D effects,
- it can later be compiled as a `.scr` application,
- it fits the user's current C# desktop development path.

Potential future alternatives:

- WPF for richer UI and effects,
- SkiaSharp for better 2D rendering,
- OpenTK/Silk.NET for OpenGL-style visualizers,
- WebView2 for HTML Canvas/WebGL effects.

## 5. Why no plugin system yet?

A real plugin system requires decisions about:

- assembly loading,
- versioning,
- configuration,
- sandboxing,
- error isolation,
- signing or trust,
- compatibility between host and plugin API.

For V0.1 this would add complexity without making the visible result better.

The project therefore uses built-in effects first. The code is modular enough that a plugin system can be added later.

## 6. Exit behavior

The prototype exits on:

- `Esc`,
- any key press,
- mouse button click,
- noticeable mouse movement after startup.

A small movement threshold prevents accidental closing immediately after the cursor is hidden.

## 7. Multi-monitor behavior

V0.1 originally used `WindowState = Maximized`, which tends to place the form on the primary Windows monitor. V0.1.1 changed the host so that each `ScreenSaverForm` receives a concrete `Screen` and applies that screen's `Bounds` manually.

The startup behavior is:

- normal developer run: use the monitor under the mouse pointer,
- `/screen:N`: use the selected zero-based screen index,
- `/all-screens`: open one fullscreen form per connected monitor,
- `/s`: prepared to cover all connected monitors for real screensaver mode.

A small `ScreenSaverApplicationContext` manages several forms and closes all screensaver windows when the user exits from any monitor.

## 8. Known technical limitations

- The render loop is timer-based, not a dedicated high-precision game loop.
- `System.Drawing` / GDI+ is sufficient for V0.1, but not ideal for advanced particle effects.
- The current version has no persistent settings.
- The project has not yet been converted into a real `.scr` screensaver file.
- Windows preview mode is parsed but not yet rendered inside the preview handle.
