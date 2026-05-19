namespace Sasd.ScreenSaverLab.Effects.Feeds;

/// <summary>
/// Represents one displayable feed-style message for the Amber Feed effect.
/// </summary>
/// <param name="Source">Short source label shown in square brackets.</param>
/// <param name="Title">Headline or status title shown prominently.</param>
/// <param name="Summary">Short explanatory text shown below the title.</param>
public sealed record AmberFeedDisplayItem(string Source, string Title, string Summary);
