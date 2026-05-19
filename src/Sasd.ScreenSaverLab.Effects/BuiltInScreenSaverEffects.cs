using Sasd.ScreenSaverLab.Core;

namespace Sasd.ScreenSaverLab.Effects;

/// <summary>
/// Creates built-in screensaver effects by name or alias.
/// </summary>
/// <remarks>
/// This is intentionally a very small factory, not a plugin system. Keeping the first
/// versions simple makes it easy to add new effects while the public effect API is still
/// evolving. V0.6.1 also centralizes the alias table so command-line parsing and factory
/// creation cannot silently drift apart.
/// </remarks>
public static class BuiltInScreenSaverEffects
{
    /// <summary>
    /// Gets the canonical name of the default effect.
    /// </summary>
    public const string DefaultEffectName = "star-drift";

    private static readonly BuiltInEffectDefinition[] Definitions =
    [
        new(
            CanonicalName: "star-drift",
            DisplayName: "Star Drift",
            Description: "Calm particle field with subtle depth impression.",
            Aliases: ["star", "stars", "star-drift"]),

        new(
            CanonicalName: "data-stream",
            DisplayName: "Data Stream",
            Description: "Falling technical glyph streams with an original SASD data-flow look.",
            Aliases: ["data", "stream", "datastream", "data-stream", "cipher", "cipherfall"]),

        new(
            CanonicalName: "light-trails",
            DisplayName: "Light Trails",
            Description: "Soft glowing light trails moving across a calm visualizer field.",
            Aliases: ["light", "lights", "trail", "trails", "light-trail", "light-trails", "lighttrail"]),

        new(
            CanonicalName: "amber-feed",
            DisplayName: "Amber Feed",
            Description: "Amber retro terminal feed display with RSS/Atom support, cache and demo fallback.",
            Aliases: ["amber", "feed", "amber-feed", "amberfeed", "retro-feed", "retrofeed"]),

        new(
            CanonicalName: "wireframe-terrain",
            DisplayName: "Wireframe Terrain",
            Description: "1980s-inspired vector landscape with moving perspective grid lines.",
            Aliases: ["wire", "wireframe", "terrain", "wireframe-terrain", "wireframeterrain", "vector-terrain", "grid-terrain"]),

        new(
            CanonicalName: "plasma-grid",
            DisplayName: "Plasma Grid",
            Description: "Retro plasma field with a restrained technical grid overlay.",
            Aliases: ["plasma", "grid", "plasma-grid", "plasmagrid", "plasma-field"]),

        new(
            CanonicalName: "orbit-field",
            DisplayName: "Orbit Field",
            Description: "Scientific particle-orbit effect with soft attractor field lines.",
            Aliases: ["orbit", "orbits", "field", "orbit-field", "orbitfield", "particle-orbit"]),

        new(
            CanonicalName: "lab-console",
            DisplayName: "Lab Console",
            Description: "Fictional laboratory and research-data console with generated sample rows.",
            Aliases: ["lab", "labs", "console", "lab-console", "labconsole", "research", "research-console"]),

        new(
            CanonicalName: "system-pulse",
            DisplayName: "System Pulse",
            Description: "Abstract local CPU, RAM and network telemetry visualization.",
            Aliases: ["pulse", "system", "system-pulse", "systempulse", "sys", "telemetry", "monitor"])
    ];

    private static readonly Dictionary<string, string> AliasToCanonicalName = BuildAliasMap();

    /// <summary>
    /// Creates a new effect instance for the requested name.
    /// </summary>
    /// <param name="effectName">Canonical effect name or user-friendly alias.</param>
    /// <param name="amberFeedConfigurationPath">Optional JSON configuration path for the Amber Feed effect.</param>
    /// <returns>A fresh effect instance. Each monitor should receive its own instance.</returns>
    public static IScreenSaverEffect Create(string? effectName, string? amberFeedConfigurationPath = null)
    {
        string canonicalName = ResolveCanonicalName(effectName);

        return canonicalName switch
        {
            "data-stream" => new DataStreamEffect(),
            "light-trails" => new LightTrailsEffect(),
            "amber-feed" => new AmberFeedEffect(amberFeedConfigurationPath),
            "wireframe-terrain" => new WireframeTerrainEffect(),
            "plasma-grid" => new PlasmaGridEffect(),
            "orbit-field" => new OrbitFieldEffect(),
            "lab-console" => new LabConsoleEffect(),
            "system-pulse" => new SystemPulseEffect(),
            "star-drift" => new StarDriftEffect(),

            // Unknown names intentionally fall back to the safe default. This prevents
            // a typo in the command line from producing a black screen or startup crash.
            _ => new StarDriftEffect()
        };
    }

    /// <summary>
    /// Resolves a canonical effect name from a raw user-facing name or alias.
    /// </summary>
    public static string ResolveCanonicalName(string? effectName)
    {
        string normalized = NormalizeEffectToken(effectName);

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return DefaultEffectName;
        }

        return AliasToCanonicalName.TryGetValue(normalized, out string? canonicalName)
            ? canonicalName
            : normalized;
    }

    /// <summary>
    /// Tries to resolve an effect selection directly from the raw command-line arguments.
    /// </summary>
    /// <remarks>
    /// The core parser already understands effect aliases. This additional resolver keeps
    /// the application robust if the parser and effect factory temporarily drift during
    /// development or if a user passes a direct shortcut such as <c>/lab</c>.
    /// </remarks>
    public static bool TryResolveFromArguments(IEnumerable<string> args, out string canonicalEffectName)
    {
        ArgumentNullException.ThrowIfNull(args);

        foreach (string rawArgument in args)
        {
            string token = NormalizeEffectToken(rawArgument);

            if (string.IsNullOrWhiteSpace(token))
            {
                continue;
            }

            // Ignore non-effect options that can carry arbitrary values.
            if (token.StartsWith("screen:", StringComparison.OrdinalIgnoreCase)
                || token.StartsWith("screen=", StringComparison.OrdinalIgnoreCase)
                || token.StartsWith("feeds:", StringComparison.OrdinalIgnoreCase)
                || token.StartsWith("feeds=", StringComparison.OrdinalIgnoreCase)
                || token.StartsWith("feed-config:", StringComparison.OrdinalIgnoreCase)
                || token.StartsWith("feed-config=", StringComparison.OrdinalIgnoreCase)
                || token.StartsWith("rss-config:", StringComparison.OrdinalIgnoreCase)
                || token.StartsWith("rss-config=", StringComparison.OrdinalIgnoreCase)
                || token.StartsWith("rss:", StringComparison.OrdinalIgnoreCase)
                || token.StartsWith("rss=", StringComparison.OrdinalIgnoreCase)
                || token.StartsWith("clock:", StringComparison.OrdinalIgnoreCase)
                || token.StartsWith("clock=", StringComparison.OrdinalIgnoreCase)
                || token.StartsWith("power:", StringComparison.OrdinalIgnoreCase)
                || token.StartsWith("power=", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (AliasToCanonicalName.TryGetValue(token, out string? canonicalName))
            {
                canonicalEffectName = canonicalName;
                return true;
            }

            string[] prefixes = ["effect:", "effect=", "visual:", "visual="];

            foreach (string prefix in prefixes)
            {
                if (token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    string value = token[prefix.Length..].Trim();

                    if (AliasToCanonicalName.TryGetValue(value, out string? canonicalFromPrefixedValue))
                    {
                        canonicalEffectName = canonicalFromPrefixedValue;
                        return true;
                    }

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        canonicalEffectName = value;
                        return true;
                    }
                }
            }
        }

        canonicalEffectName = DefaultEffectName;
        return false;
    }

    /// <summary>
    /// Returns a short user-facing list of currently supported effects.
    /// </summary>
    public static string GetSupportedEffectsText()
    {
        return string.Join(", ", Definitions.Select(definition => definition.CanonicalName));
    }

    /// <summary>
    /// Returns a manpage-friendly detailed list of built-in effects and aliases.
    /// </summary>
    public static string GetDetailedEffectsText()
    {
        return string.Join(
            Environment.NewLine,
            Definitions.Select(definition =>
                $"{definition.CanonicalName,-22} {definition.DisplayName,-20} aliases: {string.Join(", ", definition.Aliases)}"));
    }

    private static Dictionary<string, string> BuildAliasMap()
    {
        Dictionary<string, string> map = new(StringComparer.OrdinalIgnoreCase);

        foreach (BuiltInEffectDefinition definition in Definitions)
        {
            map[definition.CanonicalName] = definition.CanonicalName;

            foreach (string alias in definition.Aliases)
            {
                map[alias] = definition.CanonicalName;
            }
        }

        return map;
    }

    /// <summary>
    /// Normalizes names so aliases can be compared reliably.
    /// </summary>
    private static string NormalizeEffectToken(string? effectName)
    {
        if (string.IsNullOrWhiteSpace(effectName))
        {
            return string.Empty;
        }

        return effectName
            .Trim()
            .TrimStart('/', '-')
            .ToLowerInvariant();
    }

    private sealed record BuiltInEffectDefinition(
        string CanonicalName,
        string DisplayName,
        string Description,
        IReadOnlyList<string> Aliases);
}
