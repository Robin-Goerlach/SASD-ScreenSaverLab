using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Sasd.ScreenSaverLab.Effects.Feeds;

/// <summary>
/// Parses common RSS and Atom XML documents into Amber Feed display items.
/// </summary>
/// <remarks>
/// The parser intentionally supports a conservative subset: RSS <c>item</c> elements
/// and Atom <c>entry</c> elements. It ignores unsupported extensions rather than
/// failing the whole effect.
/// </remarks>
public static partial class AmberFeedXmlParser
{
    /// <summary>
    /// Parses a feed XML document and returns displayable feed items.
    /// </summary>
    /// <param name="sourceName">Short feed label from the configuration file.</param>
    /// <param name="xml">RSS or Atom XML text.</param>
    /// <param name="maxItems">Maximum number of items to return from this feed.</param>
    public static IReadOnlyList<AmberFeedDisplayItem> Parse(string sourceName, string xml, int maxItems)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return [];
        }

        XDocument document = LoadXmlSafely(xml);
        List<AmberFeedDisplayItem> items = [];

        IEnumerable<XElement> rssItems = document.Descendants()
            .Where(element => element.Name.LocalName.Equals("item", StringComparison.OrdinalIgnoreCase));

        foreach (XElement item in rssItems.Take(maxItems))
        {
            AmberFeedDisplayItem? displayItem = ConvertRssItem(sourceName, item);

            if (displayItem is not null)
            {
                items.Add(displayItem);
            }
        }

        if (items.Count > 0)
        {
            return items;
        }

        IEnumerable<XElement> atomEntries = document.Descendants()
            .Where(element => element.Name.LocalName.Equals("entry", StringComparison.OrdinalIgnoreCase));

        foreach (XElement entry in atomEntries.Take(maxItems))
        {
            AmberFeedDisplayItem? displayItem = ConvertAtomEntry(sourceName, entry);

            if (displayItem is not null)
            {
                items.Add(displayItem);
            }
        }

        return items;
    }


    /// <summary>
    /// Loads feed XML with DTD processing disabled to avoid external entity surprises.
    /// </summary>
    private static XDocument LoadXmlSafely(string xml)
    {
        XmlReaderSettings settings = new()
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        using StringReader stringReader = new(xml);
        using XmlReader xmlReader = XmlReader.Create(stringReader, settings);

        return XDocument.Load(xmlReader, LoadOptions.None);
    }

    /// <summary>
    /// Converts one RSS item element into a display item.
    /// </summary>
    private static AmberFeedDisplayItem? ConvertRssItem(string sourceName, XElement item)
    {
        string title = CleanText(GetChildValue(item, "title"));
        string summary = CleanText(
            GetChildValue(item, "description")
            ?? GetChildValue(item, "encoded")
            ?? GetChildValue(item, "content"));

        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(summary))
        {
            return null;
        }

        return new AmberFeedDisplayItem(
            Shorten(sourceName, 24),
            Shorten(string.IsNullOrWhiteSpace(title) ? "Untitled feed item" : title, 120),
            Shorten(summary, 220));
    }

    /// <summary>
    /// Converts one Atom entry element into a display item.
    /// </summary>
    private static AmberFeedDisplayItem? ConvertAtomEntry(string sourceName, XElement entry)
    {
        string title = CleanText(GetChildValue(entry, "title"));
        string summary = CleanText(
            GetChildValue(entry, "summary")
            ?? GetChildValue(entry, "content"));

        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(summary))
        {
            return null;
        }

        return new AmberFeedDisplayItem(
            Shorten(sourceName, 24),
            Shorten(string.IsNullOrWhiteSpace(title) ? "Untitled feed entry" : title, 120),
            Shorten(summary, 220));
    }

    /// <summary>
    /// Returns the value of the first direct child whose local XML name matches one of the requested names.
    /// </summary>
    private static string? GetChildValue(XElement parent, params string[] localNames)
    {
        foreach (XElement child in parent.Elements())
        {
            if (localNames.Any(name => child.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return child.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Removes HTML fragments and normalizes whitespace so feed snippets fit the terminal layout.
    /// </summary>
    private static string CleanText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string withoutHtml = HtmlTagRegex().Replace(text, " ");
        string decoded = WebUtility.HtmlDecode(withoutHtml);
        string normalized = WhitespaceRegex().Replace(decoded, " ").Trim();

        return normalized;
    }

    /// <summary>
    /// Shortens long feed text while keeping the terminal layout readable.
    /// </summary>
    private static string Shorten(string text, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string normalized = text.Trim();

        if (normalized.Length <= maximumLength)
        {
            return normalized;
        }

        return normalized[..Math.Max(0, maximumLength - 1)].TrimEnd() + "…";
    }

    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex("\\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();
}
