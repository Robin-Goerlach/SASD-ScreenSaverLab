using System.Text.Json.Serialization;

namespace Sasd.ScreenSaverLab.Effects.Feeds;

/// <summary>
/// JSON-mapped configuration model for <c>config/feeds.json</c>.
/// </summary>
/// <remarks>
/// V0.4.1 only loads and validates this configuration. The actual RSS/Atom download
/// and cache layer are intentionally deferred to a later iteration.
/// </remarks>
public sealed class AmberFeedConfiguration
{
    /// <summary>
    /// Gets or sets the intended refresh interval for future RSS retrieval.
    /// </summary>
    [JsonPropertyName("refreshMinutes")]
    public int RefreshMinutes { get; set; } = 15;

    /// <summary>
    /// Gets or sets the maximum number of items future feed retrieval should keep per source.
    /// </summary>
    [JsonPropertyName("maxItemsPerFeed")]
    public int MaxItemsPerFeed { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether demo items may be shown when RSS loading is unavailable.
    /// </summary>
    [JsonPropertyName("useDemoItemsWhenOffline")]
    public bool UseDemoItemsWhenOffline { get; set; } = true;

    /// <summary>
    /// Gets or sets the intended visual theme. V0.4.1 still renders the amber theme only.
    /// </summary>
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "amber";

    /// <summary>
    /// Gets or sets the configured RSS/Atom feed sources.
    /// </summary>
    [JsonPropertyName("feeds")]
    public List<AmberFeedSourceConfiguration> Feeds { get; set; } = [];
}

/// <summary>
/// JSON-mapped configuration for one RSS/Atom feed source.
/// </summary>
public sealed class AmberFeedSourceConfiguration
{
    /// <summary>
    /// Gets or sets the short user-facing feed name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the RSS/Atom feed URL.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this feed should be used by future RSS retrieval.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}
