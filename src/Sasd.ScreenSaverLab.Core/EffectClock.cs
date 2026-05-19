namespace Sasd.ScreenSaverLab.Core;

/// <summary>
/// Small injectable clock abstraction used to avoid DateTime.Now calls scattered through the code.
/// </summary>
public interface IEffectClock
{
    /// <summary>
    /// Gets the current local date and time.
    /// </summary>
    DateTime Now { get; }
}

/// <summary>
/// Default implementation that returns the computer's local time.
/// </summary>
public sealed class SystemEffectClock : IEffectClock
{
    /// <inheritdoc />
    public DateTime Now => DateTime.Now;
}
