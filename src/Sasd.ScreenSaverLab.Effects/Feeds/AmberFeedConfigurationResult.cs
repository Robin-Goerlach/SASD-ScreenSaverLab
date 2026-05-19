namespace Sasd.ScreenSaverLab.Effects.Feeds;

/// <summary>
/// Contains the result of loading the Amber Feed configuration file.
/// </summary>
/// <param name="Items">Display items derived from the configured feed sources.</param>
/// <param name="StatusLine">Short terminal status text shown in the Amber Feed footer.</param>
/// <param name="LoadedFromConfiguration">True when a configuration file was found and parsed successfully.</param>
public sealed record AmberFeedConfigurationResult(
    IReadOnlyList<AmberFeedDisplayItem> Items,
    string StatusLine,
    bool LoadedFromConfiguration);
