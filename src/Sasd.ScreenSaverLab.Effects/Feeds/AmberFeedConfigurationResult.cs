namespace Sasd.ScreenSaverLab.Effects.Feeds;

/// <summary>
/// Contains the result of loading the Amber Feed configuration file.
/// </summary>
/// <param name="Items">Display items derived from the configured feed sources or warnings.</param>
/// <param name="StatusLine">Short terminal status text shown in the Amber Feed footer.</param>
/// <param name="LoadedFromConfiguration">True when a configuration file was found and parsed successfully.</param>
/// <param name="Configuration">Parsed configuration object, if available.</param>
/// <param name="EnabledFeeds">Validated, enabled HTTP/HTTPS feed sources.</param>
/// <param name="ResolvedConfigurationPath">Resolved absolute configuration file path, if available.</param>
public sealed record AmberFeedConfigurationResult(
    IReadOnlyList<AmberFeedDisplayItem> Items,
    string StatusLine,
    bool LoadedFromConfiguration,
    AmberFeedConfiguration? Configuration = null,
    IReadOnlyList<AmberFeedSourceConfiguration>? EnabledFeeds = null,
    string? ResolvedConfigurationPath = null)
{
    /// <summary>
    /// Gets whether at least one enabled feed source can be used for live RSS/Atom retrieval.
    /// </summary>
    public bool HasEnabledFeeds => EnabledFeeds is { Count: > 0 };
}
