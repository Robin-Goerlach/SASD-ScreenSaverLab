using System.Text.Json;

namespace Sasd.ScreenSaverLab.Effects.Feeds;

/// <summary>
/// Reads and writes a small local cache for Amber Feed items.
/// </summary>
/// <remarks>
/// The cache is deliberately best-effort. A screensaver should never fail because the
/// cache file is missing, locked, corrupt or not writable. All such errors are swallowed
/// and converted into an empty result.
/// </remarks>
public static class AmberFeedCacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Loads cached Amber Feed items from the user's local application-data folder.
    /// </summary>
    public static IReadOnlyList<AmberFeedDisplayItem> LoadItems()
    {
        try
        {
            string path = GetCacheFilePath();

            if (!File.Exists(path))
            {
                return [];
            }

            string json = File.ReadAllText(path);
            AmberFeedCacheDocument? document = JsonSerializer.Deserialize<AmberFeedCacheDocument>(json, SerializerOptions);

            return document?.Items
                .Where(item => !string.IsNullOrWhiteSpace(item.Title))
                .Take(100)
                .ToList() ?? [];
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return [];
        }
    }

    /// <summary>
    /// Stores the most recent Amber Feed items for later offline fallback.
    /// </summary>
    public static void SaveItems(IEnumerable<AmberFeedDisplayItem> items)
    {
        try
        {
            List<AmberFeedDisplayItem> safeItems = items
                .Where(item => !string.IsNullOrWhiteSpace(item.Title))
                .Take(100)
                .ToList();

            if (safeItems.Count == 0)
            {
                return;
            }

            string path = GetCacheFilePath();
            string? directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            AmberFeedCacheDocument document = new()
            {
                CreatedUtc = DateTimeOffset.UtcNow,
                Items = safeItems
            };

            string json = JsonSerializer.Serialize(document, SerializerOptions);
            File.WriteAllText(path, json);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            // Best-effort cache only. The visual effect must keep running even when the
            // cache cannot be written, for example on a locked-down workstation.
        }
    }

    /// <summary>
    /// Returns the default cache file path in the current user's local app data folder.
    /// </summary>
    public static string GetCacheFilePath()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (string.IsNullOrWhiteSpace(localAppData))
        {
            localAppData = AppContext.BaseDirectory;
        }

        return Path.Combine(localAppData, "SASD", "ScreenSaverLab", "amber-feed-cache.json");
    }
}
