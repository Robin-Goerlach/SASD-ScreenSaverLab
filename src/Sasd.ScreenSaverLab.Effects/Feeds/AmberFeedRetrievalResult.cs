namespace Sasd.ScreenSaverLab.Effects.Feeds;

/// <summary>
/// Contains the result of a live RSS/Atom retrieval attempt.
/// </summary>
/// <param name="Items">Feed items that can be rendered by the Amber Feed effect.</param>
/// <param name="StatusLine">Short status text suitable for the terminal footer.</param>
/// <param name="LoadedFromNetwork">True when at least one item came from a live feed request.</param>
public sealed record AmberFeedRetrievalResult(
    IReadOnlyList<AmberFeedDisplayItem> Items,
    string StatusLine,
    bool LoadedFromNetwork);
