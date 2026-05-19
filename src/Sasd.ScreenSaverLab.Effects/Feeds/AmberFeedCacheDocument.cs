using System.Text.Json.Serialization;

namespace Sasd.ScreenSaverLab.Effects.Feeds;

/// <summary>
/// Simple JSON cache document for recently downloaded Amber Feed items.
/// </summary>
public sealed class AmberFeedCacheDocument
{
    /// <summary>
    /// Gets or sets the UTC timestamp when the cache file was last written.
    /// </summary>
    [JsonPropertyName("createdUtc")]
    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the cached feed display items.
    /// </summary>
    [JsonPropertyName("items")]
    public List<AmberFeedDisplayItem> Items { get; set; } = [];
}
