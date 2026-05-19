using System.Text.Json;

namespace Sasd.ScreenSaverLab.Effects.Feeds;

/// <summary>
/// Loads and validates the Amber Feed JSON configuration file.
/// </summary>
/// <remarks>
/// The loader deliberately does not retrieve RSS/Atom content yet. V0.4.1 proves that
/// the configuration file can be found, parsed and represented in the visual effect.
/// Network access, caching, timeout handling and malformed feed recovery remain separate
/// follow-up steps.
/// </remarks>
public static class AmberFeedConfigurationLoader
{
    /// <summary>
    /// Default relative path for the Amber Feed configuration file.
    /// </summary>
    public const string DefaultConfigurationPath = "config/feeds.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Loads the configured feed sources and converts them into displayable placeholder items.
    /// </summary>
    /// <param name="configurationPath">Optional path to the JSON configuration file.</param>
    /// <returns>A load result that is safe to use by the visual effect.</returns>
    public static AmberFeedConfigurationResult Load(string? configurationPath)
    {
        string requestedPath = string.IsNullOrWhiteSpace(configurationPath)
            ? DefaultConfigurationPath
            : configurationPath.Trim();

        string? resolvedPath = ResolveConfigurationPath(requestedPath);

        if (resolvedPath is null)
        {
            return new AmberFeedConfigurationResult(
                [],
                $"DEMO MODE  //  CONFIG NOT FOUND: {requestedPath}",
                LoadedFromConfiguration: false);
        }

        try
        {
            string json = File.ReadAllText(resolvedPath);
            AmberFeedConfiguration? configuration = JsonSerializer.Deserialize<AmberFeedConfiguration>(json, SerializerOptions);

            if (configuration is null)
            {
                return new AmberFeedConfigurationResult(
                    [],
                    $"DEMO MODE  //  CONFIG EMPTY: {ShortenPath(resolvedPath)}",
                    LoadedFromConfiguration: false);
            }

            return BuildResult(configuration, resolvedPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return new AmberFeedConfigurationResult(
                [new AmberFeedDisplayItem("CONFIG", "Amber Feed configuration could not be loaded", ex.Message)],
                $"DEMO MODE  //  CONFIG ERROR: {ShortenPath(resolvedPath)}",
                LoadedFromConfiguration: false);
        }
    }

    /// <summary>
    /// Converts a parsed configuration file into placeholder display items.
    /// </summary>
    private static AmberFeedConfigurationResult BuildResult(AmberFeedConfiguration configuration, string resolvedPath)
    {
        List<AmberFeedSourceConfiguration> configuredFeeds = configuration.Feeds ?? [];
        int configuredCount = configuredFeeds.Count;
        List<AmberFeedDisplayItem> items = [];
        List<string> warnings = [];

        foreach (AmberFeedSourceConfiguration feed in configuredFeeds)
        {
            string name = string.IsNullOrWhiteSpace(feed.Name) ? "Unnamed Feed" : feed.Name.Trim();
            string url = feed.Url?.Trim() ?? string.Empty;

            if (!feed.Enabled)
            {
                continue;
            }

            if (!IsHttpFeedUrl(url))
            {
                warnings.Add(name);
                continue;
            }

            items.Add(new AmberFeedDisplayItem(
                name,
                "RSS source configured",
                $"{url} // refresh target {Math.Max(1, configuration.RefreshMinutes)} min // max {Math.Max(1, configuration.MaxItemsPerFeed)} items // retrieval planned for V0.4.2"));
        }

        string status = $"CONFIG LOADED  //  {items.Count}/{configuredCount} SOURCES ENABLED  //  {ShortenPath(resolvedPath)}";

        if (warnings.Count > 0)
        {
            items.Add(new AmberFeedDisplayItem(
                "CONFIG",
                "Some configured RSS sources were ignored",
                $"Invalid or unsupported URL in: {string.Join(", ", warnings.Take(3))}"));

            status += "  //  WARNINGS";
        }

        if (items.Count == 0 && configuration.UseDemoItemsWhenOffline)
        {
            status += "  //  DEMO FALLBACK";
        }

        return new AmberFeedConfigurationResult(items, status, LoadedFromConfiguration: true);
    }

    /// <summary>
    /// Accepts only absolute HTTP and HTTPS feed URLs.
    /// </summary>
    private static bool IsHttpFeedUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? parsed)
            && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Resolves a configuration path for both repository runs and published application runs.
    /// </summary>
    private static string? ResolveConfigurationPath(string requestedPath)
    {
        if (Path.IsPathRooted(requestedPath))
        {
            return File.Exists(requestedPath) ? requestedPath : null;
        }

        List<string> candidates =
        [
            Path.GetFullPath(requestedPath, Directory.GetCurrentDirectory()),
            Path.GetFullPath(requestedPath, AppContext.BaseDirectory)
        ];

        DirectoryInfo? current = new(AppContext.BaseDirectory);

        for (int i = 0; i < 6 && current is not null; i++)
        {
            candidates.Add(Path.GetFullPath(requestedPath, current.FullName));
            current = current.Parent;
        }

        return candidates.FirstOrDefault(File.Exists);
    }

    /// <summary>
    /// Keeps the terminal status line readable even for longer absolute paths.
    /// </summary>
    private static string ShortenPath(string path)
    {
        string fileName = Path.GetFileName(path);
        string? parent = Path.GetFileName(Path.GetDirectoryName(path));

        if (!string.IsNullOrWhiteSpace(parent))
        {
            return $"{parent}/{fileName}";
        }

        return fileName;
    }
}
