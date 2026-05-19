using System.Drawing;

namespace Sasd.ScreenSaverLab.Core;

/// <summary>
/// Defines the small contract every built-in screensaver effect must implement.
/// </summary>
/// <remarks>
/// The interface is intentionally compact for V0.1. A real plugin API can be
/// introduced later, once several effects exist and the requirements are clearer.
/// </remarks>
public interface IScreenSaverEffect
{
    /// <summary>
    /// Gets the short display name of the effect.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a human-readable description of the effect.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Initializes the effect for the current viewport size.
    /// </summary>
    /// <param name="viewportSize">The current drawing area.</param>
    void Initialize(Size viewportSize);

    /// <summary>
    /// Updates the internal animation state of the effect.
    /// </summary>
    /// <param name="elapsed">The time since the previous update.</param>
    /// <param name="viewportSize">The current drawing area.</param>
    void Update(TimeSpan elapsed, Size viewportSize);

    /// <summary>
    /// Renders the current frame of the effect.
    /// </summary>
    /// <param name="graphics">The GDI+ graphics context supplied by the host form.</param>
    /// <param name="viewportSize">The current drawing area.</param>
    void Render(Graphics graphics, Size viewportSize);
}
