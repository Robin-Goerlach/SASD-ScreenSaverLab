using System.Text.Json.Serialization;

namespace Sasd.ScreenSaverLab.Effects.Feeds;

/// <summary>
/// JSON-mapped configuration model for <c>config/feeds.json</c>.
/// </summary>
/// <remarks>
/// The configuration intentionally stays small and human-editable. It describes which
/// RSS/Atom feeds should be used, how many items should be kept per feed and how
/// aggressively network requests may run while the screensaver is visible.
/// </remarks>
public sealed class AmberFeedConfiguration
{
    /// <summary>
    /// Gets or sets the intended refresh interval for RSS retrieval.
    /// </summary>
    [JsonPropertyName("refreshMinutes")]
    public int RefreshMinutes { get; set; } = 15;

    /// <summary>
    /// Gets or sets the maximum number of items the retrieval layer should keep per source.
    /// </summary>
    [JsonPropertyName("maxItemsPerFeed")]
    public int MaxItemsPerFeed { get; set; } = 10;

    /// <summary>
    /// Gets or sets the per-feed network timeout in seconds.
    /// </summary>
    /// <remarks>
    /// Screensavers must remain responsive. A short timeout is safer than letting a slow
    /// feed block refresh attempts for a long time.
    /// </remarks>
    [JsonPropertyName("requestTimeoutSeconds")]
    public int RequestTimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether demo items may be shown when RSS loading is unavailable.
    /// </summary>
    [JsonPropertyName("useDemoItemsWhenOffline")]
    public bool UseDemoItemsWhenOffline { get; set; } = true;

    /// <summary>
    /// Gets or sets how many feed items are displayed on one terminal page.
    /// </summary>
    /// <remarks>
    /// A smaller page is easier to read on a screensaver. The effect clamps unreasonable
    /// values at runtime so a broken configuration does not crash rendering.
    /// </remarks>
    [JsonPropertyName("itemsPerPage")]
    public int ItemsPerPage { get; set; } = 3;

    /// <summary>
    /// Gets or sets how long one terminal page remains visible before switching.
    /// </summary>
    [JsonPropertyName("pageDurationSeconds")]
    public double PageDurationSeconds { get; set; } = 28.0;

    /// <summary>
    /// Gets or sets how many characters per second are revealed by the typewriter animation.
    /// </summary>
    [JsonPropertyName("characterRevealRate")]
    public double CharacterRevealRate { get; set; } = 28.0;

    /// <summary>
    /// Gets or sets the intended visual theme. V0.4.3 still renders the amber theme only.
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
    /// Gets or sets whether this feed should be used by RSS retrieval.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}
