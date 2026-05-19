namespace Sasd.ScreenSaverLab.Effects.Feeds;

/// <summary>
/// Downloads configured RSS/Atom feeds and converts them into Amber Feed display items.
/// </summary>
/// <remarks>
/// The loader is intentionally timeout-safe and best-effort. A broken feed should never
/// crash the screensaver or block rendering. Failed sources are counted in the status
/// line, and callers can fall back to cached or demo items.
/// </remarks>
public static class AmberFeedRssLoader
{
    /// <summary>
    /// Downloads all enabled RSS/Atom sources from the configuration result.
    /// </summary>
    /// <param name="configurationResult">Validated configuration load result.</param>
    /// <param name="cancellationToken">Cancellation token used when the application closes.</param>
    public static async Task<AmberFeedRetrievalResult> LoadAsync(
        AmberFeedConfigurationResult configurationResult,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(configurationResult);

        AmberFeedConfiguration configuration = configurationResult.Configuration ?? new AmberFeedConfiguration();
        IReadOnlyList<AmberFeedSourceConfiguration> sources = configurationResult.EnabledFeeds ?? [];

        if (sources.Count == 0)
        {
            return new AmberFeedRetrievalResult(
                [],
                "RSS DISABLED  //  NO ENABLED FEED SOURCES",
                LoadedFromNetwork: false);
        }

        int maxItemsPerFeed = Math.Clamp(configuration.MaxItemsPerFeed, 1, 50);
        int timeoutSeconds = Math.Clamp(configuration.RequestTimeoutSeconds, 1, 30);
        List<AmberFeedDisplayItem> allItems = [];
        int successfulSources = 0;
        int failedSources = 0;

        using HttpClient httpClient = CreateHttpClient(timeoutSeconds);

        foreach (AmberFeedSourceConfiguration source in sources)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                IReadOnlyList<AmberFeedDisplayItem> sourceItems = await LoadSingleFeedAsync(
                    httpClient,
                    source,
                    maxItemsPerFeed,
                    timeoutSeconds,
                    cancellationToken).ConfigureAwait(false);

                if (sourceItems.Count > 0)
                {
                    allItems.AddRange(sourceItems);
                    successfulSources++;
                }
                else
                {
                    failedSources++;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                failedSources++;
            }
        }

        if (allItems.Count > 0)
        {
            List<AmberFeedDisplayItem> limitedItems = allItems.Take(100).ToList();
            AmberFeedCacheService.SaveItems(limitedItems);

            string status = $"RSS LIVE  //  {limitedItems.Count} ITEMS  //  {successfulSources}/{sources.Count} SOURCES  //  {DateTime.Now:HH:mm}";

            if (failedSources > 0)
            {
                status += "  //  SOME SOURCES FAILED";
            }

            return new AmberFeedRetrievalResult(limitedItems, status, LoadedFromNetwork: true);
        }

        return new AmberFeedRetrievalResult(
            [],
            $"RSS OFFLINE  //  0 ITEMS  //  {failedSources}/{sources.Count} SOURCES FAILED",
            LoadedFromNetwork: false);
    }

    /// <summary>
    /// Downloads and parses one RSS/Atom source.
    /// </summary>
    private static async Task<IReadOnlyList<AmberFeedDisplayItem>> LoadSingleFeedAsync(
        HttpClient httpClient,
        AmberFeedSourceConfiguration source,
        int maxItems,
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        using HttpResponseMessage response = await httpClient.GetAsync(source.Url, timeoutSource.Token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        string xml = await response.Content.ReadAsStringAsync(timeoutSource.Token).ConfigureAwait(false);
        return AmberFeedXmlParser.Parse(source.Name, xml, maxItems);
    }

    /// <summary>
    /// Creates a conservative HTTP client for feed retrieval.
    /// </summary>
    private static HttpClient CreateHttpClient(int timeoutSeconds)
    {
        HttpClient httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(timeoutSeconds + 1)
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SASD-ScreenSaverLab/0.4.5");
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/rss+xml");
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/atom+xml");
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/xml");
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/xml");

        return httpClient;
    }
}
