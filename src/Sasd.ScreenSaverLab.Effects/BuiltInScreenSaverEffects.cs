using Sasd.ScreenSaverLab.Core;

namespace Sasd.ScreenSaverLab.Effects;

/// <summary>
/// Creates built-in screensaver effects by name or alias.
/// </summary>
/// <remarks>
/// This is intentionally a very small factory, not a plugin system. Keeping the first
/// versions simple makes it easy to add new effects while the public effect API is still
/// evolving. A later plugin loader can be introduced once several built-in effects have
/// proven which metadata and lifecycle hooks are actually needed.
/// </remarks>
public static class BuiltInScreenSaverEffects
{
    /// <summary>
    /// Gets the canonical name of the default effect.
    /// </summary>
    public const string DefaultEffectName = "star-drift";

    /// <summary>
    /// Creates a new effect instance for the requested name.
    /// </summary>
    /// <param name="effectName">Canonical effect name or user-friendly alias.</param>
    /// <returns>A fresh effect instance. Each monitor should receive its own instance.</returns>
    public static IScreenSaverEffect Create(string? effectName)
    {
        string normalized = NormalizeEffectName(effectName);

        return normalized switch
        {
            "data-stream" or "datastream" or "data" or "stream" or "cipherfall" or "cipher" => new DataStreamEffect(),
            "light-trails" or "lighttrail" or "light-trail" or "trails" or "trail" or "light" or "lights" => new LightTrailsEffect(),
            "amber-feed" or "amberfeed" or "amber" or "feed" or "retro-feed" or "retrofeed" => new AmberFeedEffect(),
            "star-drift" or "star" or "stars" => new StarDriftEffect(),

            // Unknown names intentionally fall back to the safe default. This prevents
            // a typo in the command line from producing a black screen or startup crash.
            _ => new StarDriftEffect()
        };
    }

    /// <summary>
    /// Returns a short user-facing list of currently supported effects.
    /// </summary>
    public static string GetSupportedEffectsText()
    {
        return "star-drift, data-stream, light-trails, amber-feed";
    }

    /// <summary>
    /// Normalizes names so aliases can be compared reliably.
    /// </summary>
    private static string NormalizeEffectName(string? effectName)
    {
        if (string.IsNullOrWhiteSpace(effectName))
        {
            return DefaultEffectName;
        }

        return effectName
            .Trim()
            .TrimStart('/', '-')
            .ToLowerInvariant();
    }
}
