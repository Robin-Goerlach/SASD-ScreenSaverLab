using System.Drawing;
using System.Drawing.Drawing2D;
using Sasd.ScreenSaverLab.Core;
using Sasd.ScreenSaverLab.Effects.Feeds;

namespace Sasd.ScreenSaverLab.Effects;

/// <summary>
/// Displays rotating feed-style items in a warm amber retro-terminal style.
/// </summary>
/// <remarks>
/// Amber Feed can load <c>config/feeds.json</c>, show configured sources immediately and
/// then refresh live RSS/Atom items in the background. Network failures are non-fatal:
/// the effect falls back to cached or demo items so the animation remains stable.
/// </remarks>
public sealed class AmberFeedEffect : IScreenSaverEffect
{
    private const float DefaultPageDurationSeconds = 28f;
    private const float DefaultCharacterRevealRate = 28f;
    private const int DefaultItemsPerPage = 5;
    private const int DefaultMinItemsPerPage = 3;
    private const int DefaultMaxItemsPerPage = 8;
    private const bool DefaultAutoFillPage = true;

    private static readonly Color BackgroundColor = Color.FromArgb(255, 3, 2, 0);
    private static readonly Color DarkAmberColor = Color.FromArgb(255, 82, 47, 0);
    private static readonly Color AmberColor = Color.FromArgb(255, 255, 176, 40);
    private static readonly Color BrightAmberColor = Color.FromArgb(255, 255, 223, 128);
    private static readonly Color DimAmberColor = Color.FromArgb(255, 155, 91, 12);

    private readonly Random _random = new();
    private readonly object _itemSync = new();
    private readonly AmberFeedConfigurationResult _configurationResult;
    private readonly CancellationTokenSource _feedRefreshCancellation = new();
    private readonly float _pageDurationSeconds;
    private readonly float _characterRevealRate;
    private readonly int _configuredItemsPerPage;
    private readonly int _minItemsPerPage;
    private readonly int _maxItemsPerPage;
    private readonly bool _autoFillPage;

    private List<FeedItem> _items;
    private string _statusLine;
    private bool _feedRefreshStarted;

    private float _elapsedOnPage;
    private float _totalElapsed;
    private int _pageIndex;
    private int _visibleCharacters;
    private float _flicker;


    /// <summary>
    /// Initializes a new Amber Feed effect using the default configuration file path.
    /// </summary>
    public AmberFeedEffect()
        : this(AmberFeedConfigurationLoader.Load(AmberFeedConfigurationLoader.DefaultConfigurationPath))
    {
    }

    /// <summary>
    /// Initializes a new Amber Feed effect using the provided configuration path.
    /// </summary>
    /// <param name="configurationPath">Path to the Amber Feed JSON configuration file.</param>
    public AmberFeedEffect(string? configurationPath)
        : this(AmberFeedConfigurationLoader.Load(configurationPath))
    {
    }

    /// <summary>
    /// Initializes a new Amber Feed effect from a preloaded configuration result.
    /// </summary>
    /// <param name="configurationResult">Validated configuration load result.</param>
    private AmberFeedEffect(AmberFeedConfigurationResult configurationResult)
    {
        ArgumentNullException.ThrowIfNull(configurationResult);

        _configurationResult = configurationResult;
        _pageDurationSeconds = ResolvePageDurationSeconds(configurationResult.Configuration);
        _characterRevealRate = ResolveCharacterRevealRate(configurationResult.Configuration);
        _configuredItemsPerPage = ResolveItemsPerPage(configurationResult.Configuration);
        _minItemsPerPage = ResolveMinItemsPerPage(configurationResult.Configuration);
        _maxItemsPerPage = ResolveMaxItemsPerPage(configurationResult.Configuration, _minItemsPerPage);
        _autoFillPage = configurationResult.Configuration?.AutoFillPage ?? DefaultAutoFillPage;

        IReadOnlyList<AmberFeedDisplayItem> cachedItems = configurationResult.HasEnabledFeeds
            ? AmberFeedCacheService.LoadItems()
            : [];

        if (cachedItems.Count > 0)
        {
            _items = ConvertDisplayItems(cachedItems);
            _statusLine = $"CACHE LOADED  //  {cachedItems.Count} ITEMS  //  RSS REFRESH STARTING";
        }
        else
        {
            _items = configurationResult.Items.Count > 0
                ? ConvertDisplayItems(configurationResult.Items)
                : CreateDemoItems();

            _statusLine = configurationResult.HasEnabledFeeds
                ? $"{configurationResult.StatusLine}  //  RSS REFRESH STARTING"
                : configurationResult.StatusLine;
        }
    }

    /// <inheritdoc />
    public string Name => "Amber Feed";

    /// <inheritdoc />
    public string Description => "Amber retro terminal feed display with timeout-safe RSS/Atom retrieval and teletext-like motion.";

    /// <inheritdoc />
    public void Initialize(Size viewportSize)
    {
        _elapsedOnPage = 0f;
        _totalElapsed = 0f;
        _pageIndex = 0;
        _visibleCharacters = 0;
        _flicker = 0f;

        StartFeedRefreshIfNeeded();
    }

    /// <inheritdoc />
    public void Update(TimeSpan elapsed, Size viewportSize)
    {
        float seconds = MathF.Min((float)elapsed.TotalSeconds, 0.12f);

        _elapsedOnPage += seconds;
        _totalElapsed += seconds;
        _flicker = 0.5f + RandomRange(-0.08f, 0.08f);

        int pageCount = CalculatePageCount(viewportSize);

        if (_pageIndex >= pageCount)
        {
            _pageIndex = 0;
        }

        if (_elapsedOnPage >= _pageDurationSeconds)
        {
            _elapsedOnPage = 0f;
            _visibleCharacters = 0;
            _pageIndex = (_pageIndex + 1) % pageCount;
        }

        _visibleCharacters = Math.Max(_visibleCharacters, (int)(_elapsedOnPage * _characterRevealRate));
    }

    /// <inheritdoc />
    public void Render(Graphics graphics, Size viewportSize)
    {
        if (viewportSize.Width <= 0 || viewportSize.Height <= 0)
        {
            return;
        }

        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        graphics.Clear(BackgroundColor);

        DrawBackgroundGlow(graphics, viewportSize);
        DrawScanLines(graphics, viewportSize);

        RectangleF terminalBounds = CalculateTerminalBounds(viewportSize);
        DrawTerminalFrame(graphics, terminalBounds);
        DrawTerminalContent(graphics, terminalBounds);
        DrawCursor(graphics, terminalBounds);
        DrawVignette(graphics, viewportSize);
    }

    /// <summary>
    /// Draws a very dark amber radial glow so the screen does not look completely flat.
    /// </summary>
    private static void DrawBackgroundGlow(Graphics graphics, Size viewportSize)
    {
        using GraphicsPath path = new();
        path.AddEllipse(
            -viewportSize.Width * 0.2f,
            -viewportSize.Height * 0.25f,
            viewportSize.Width * 1.4f,
            viewportSize.Height * 1.5f);

        using PathGradientBrush brush = new(path)
        {
            CenterColor = Color.FromArgb(72, 92, 51, 0),
            SurroundColors = [BackgroundColor]
        };

        graphics.FillPath(brush, path);
    }

    /// <summary>
    /// Draws subtle horizontal scanlines inspired by old CRT/terminal displays.
    /// </summary>
    private static void DrawScanLines(Graphics graphics, Size viewportSize)
    {
        using Pen dimLinePen = new(Color.FromArgb(24, 255, 176, 40), 1f);
        using Pen darkLinePen = new(Color.FromArgb(34, 0, 0, 0), 1f);

        for (int y = 0; y < viewportSize.Height; y += 4)
        {
            graphics.DrawLine(dimLinePen, 0, y, viewportSize.Width, y);
            graphics.DrawLine(darkLinePen, 0, y + 2, viewportSize.Width, y + 2);
        }
    }

    /// <summary>
    /// Calculates a centered terminal area with a comfortable margin.
    /// </summary>
    private static RectangleF CalculateTerminalBounds(Size viewportSize)
    {
        float margin = Math.Max(36f, Math.Min(viewportSize.Width, viewportSize.Height) * 0.055f);

        return new RectangleF(
            margin,
            margin,
            Math.Max(1f, viewportSize.Width - margin * 2f),
            Math.Max(1f, viewportSize.Height - margin * 2f));
    }

    /// <summary>
    /// Draws the amber terminal frame and inner separator lines.
    /// </summary>
    private void DrawTerminalFrame(Graphics graphics, RectangleF terminalBounds)
    {
        int pulse = (int)(12 + MathF.Sin(_totalElapsed * 3.7f) * 5f + _flicker * 8f);

        using Pen outerGlowPen = new(Color.FromArgb(82 + pulse, 255, 157, 27), 4f);
        using Pen framePen = new(Color.FromArgb(210, 255, 176, 40), 1.5f);
        using Pen dimPen = new(Color.FromArgb(115, 155, 91, 12), 1f);

        graphics.DrawRectangle(
            outerGlowPen,
            terminalBounds.X,
            terminalBounds.Y,
            terminalBounds.Width,
            terminalBounds.Height);

        graphics.DrawRectangle(
            framePen,
            terminalBounds.X + 4f,
            terminalBounds.Y + 4f,
            terminalBounds.Width - 8f,
            terminalBounds.Height - 8f);

        float headerY = terminalBounds.Y + 54f;
        float footerY = terminalBounds.Bottom - 44f;

        graphics.DrawLine(dimPen, terminalBounds.X + 18f, headerY, terminalBounds.Right - 18f, headerY);
        graphics.DrawLine(dimPen, terminalBounds.X + 18f, footerY, terminalBounds.Right - 18f, footerY);
    }

    /// <summary>
    /// Draws the header, feed items and footer text.
    /// </summary>
    private void DrawTerminalContent(Graphics graphics, RectangleF terminalBounds)
    {
        using Font headerFont = CreateMonospaceFont(18f, FontStyle.Bold);
        using Font bodyFont = CreateMonospaceFont(CalculateBodyFontSize(terminalBounds), FontStyle.Regular);
        using Font sourceFont = CreateMonospaceFont(CalculateBodyFontSize(terminalBounds) * 0.86f, FontStyle.Bold);
        using Font footerFont = CreateMonospaceFont(11f, FontStyle.Regular);

        using Brush brightBrush = new SolidBrush(ColorWithFlicker(BrightAmberColor, 245));
        using Brush amberBrush = new SolidBrush(ColorWithFlicker(AmberColor, 218));
        using Brush dimBrush = new SolidBrush(ColorWithFlicker(DimAmberColor, 185));
        using Brush darkBrush = new SolidBrush(ColorWithFlicker(DarkAmberColor, 165));

        string header = $"SASD AMBER FEED TERMINAL  //  PAGE {101 + _pageIndex:D3}";
        string statusLine = GetStatusLineSnapshot();
        string status = $"{statusLine}  //  {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        float contentX = terminalBounds.X + 30f;
        float contentWidth = terminalBounds.Width - 60f;
        float contentBottom = terminalBounds.Bottom - 76f;

        DrawSingleLineClipped(
            graphics,
            header,
            headerFont,
            brightBrush,
            terminalBounds.X + 28f,
            terminalBounds.Y + 20f,
            terminalBounds.Width - 56f,
            headerFont.GetHeight(graphics) + 4f);

        DrawSingleLineClipped(
            graphics,
            status,
            footerFont,
            dimBrush,
            terminalBounds.X + 28f,
            terminalBounds.Bottom - 31f,
            terminalBounds.Width - 100f,
            footerFont.GetHeight(graphics) + 4f);

        List<FeedItem> pageItems = GetCurrentPageItems(terminalBounds);
        float y = terminalBounds.Y + 78f;
        float lineHeight = bodyFont.GetHeight(graphics) + 5f;
        int remainingCharacters = _visibleCharacters;

        foreach (FeedItem item in pageItems)
        {
            string sourceLine = $"[{item.Source}]";
            string titleLine = item.Title.ToUpperInvariant();
            string summaryLine = item.Summary;

            if (remainingCharacters <= 0 || y >= contentBottom)
            {
                break;
            }

            string visibleSource = RevealText(sourceLine, ref remainingCharacters);
            DrawSingleLineClipped(graphics, visibleSource, sourceFont, brightBrush, contentX, y, contentWidth, lineHeight);
            y += lineHeight * 0.9f;

            string visibleTitle = RevealText(titleLine, ref remainingCharacters);
            DrawWrappedText(
                graphics,
                visibleTitle,
                bodyFont,
                amberBrush,
                contentX,
                ref y,
                contentWidth,
                lineHeight,
                maxLines: 2,
                contentBottom);

            string visibleSummary = RevealText(summaryLine, ref remainingCharacters);
            DrawWrappedText(
                graphics,
                visibleSummary,
                footerFont,
                dimBrush,
                contentX,
                ref y,
                contentWidth,
                footerFont.GetHeight(graphics) + 4f,
                maxLines: 2,
                contentBottom);

            if (y + 28f >= contentBottom)
            {
                break;
            }

            y += 16f;

            using Pen separatorPen = new(Color.FromArgb(74, 255, 176, 40), 1f);
            graphics.DrawLine(separatorPen, contentX, y, terminalBounds.Right - 30f, y);
            y += 16f;
        }

        DrawPageProgress(graphics, terminalBounds, darkBrush, amberBrush);
    }

    /// <summary>
    /// Draws a small animated progress marker near the bottom of the terminal.
    /// </summary>
    private void DrawPageProgress(Graphics graphics, RectangleF terminalBounds, Brush dimBrush, Brush amberBrush)
    {
        float progress = Math.Clamp(_elapsedOnPage / _pageDurationSeconds, 0f, 1f);
        float barX = terminalBounds.X + 28f;
        float barY = terminalBounds.Bottom - 61f;
        float barWidth = terminalBounds.Width - 56f;
        float barHeight = 5f;

        graphics.FillRectangle(dimBrush, barX, barY, barWidth, barHeight);
        graphics.FillRectangle(amberBrush, barX, barY, barWidth * progress, barHeight);
    }

    /// <summary>
    /// Draws a blinking cursor in the lower-right terminal area.
    /// </summary>
    private void DrawCursor(Graphics graphics, RectangleF terminalBounds)
    {
        bool cursorVisible = ((int)(_totalElapsed * 2.0f) % 2) == 0;

        if (!cursorVisible)
        {
            return;
        }

        using Brush cursorBrush = new SolidBrush(Color.FromArgb(210, BrightAmberColor));
        graphics.FillRectangle(cursorBrush, terminalBounds.Right - 46f, terminalBounds.Bottom - 32f, 14f, 20f);
    }

    /// <summary>
    /// Darkens the edges so the terminal feels like an old monitor surface.
    /// </summary>
    private static void DrawVignette(Graphics graphics, Size viewportSize)
    {
        using GraphicsPath path = new();
        path.AddEllipse(
            -viewportSize.Width * 0.35f,
            -viewportSize.Height * 0.45f,
            viewportSize.Width * 1.7f,
            viewportSize.Height * 1.9f);

        using PathGradientBrush brush = new(path)
        {
            CenterColor = Color.FromArgb(0, 0, 0, 0),
            SurroundColors = [Color.FromArgb(185, 0, 0, 0)]
        };

        graphics.FillRectangle(brush, new Rectangle(0, 0, viewportSize.Width, viewportSize.Height));
    }

    /// <summary>
    /// Draws a single line inside a clipping rectangle and adds an ellipsis if the text is too long.
    /// </summary>
    private static void DrawSingleLineClipped(
        Graphics graphics,
        string text,
        Font font,
        Brush brush,
        float x,
        float y,
        float width,
        float height)
    {
        if (string.IsNullOrEmpty(text) || width <= 1f || height <= 1f)
        {
            return;
        }

        RectangleF layoutRectangle = new(x, y, width, height);

        using StringFormat format = new(StringFormatFlags.NoWrap)
        {
            Trimming = StringTrimming.EllipsisCharacter,
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near
        };

        graphics.DrawString(text, font, brush, layoutRectangle, format);
    }

    /// <summary>
    /// Draws wrapped text inside the terminal content area.
    /// </summary>
    /// <remarks>
    /// RSS feeds can contain very long titles, URLs or descriptions without useful
    /// whitespace. Drawing into a bounded layout rectangle prevents those strings from
    /// bleeding over the terminal frame. The method also respects the available vertical
    /// content area so long entries cannot overwrite the progress bar or footer.
    /// </remarks>
    private static void DrawWrappedText(
        Graphics graphics,
        string text,
        Font font,
        Brush brush,
        float x,
        ref float y,
        float width,
        float lineHeight,
        int maxLines,
        float bottom)
    {
        if (string.IsNullOrWhiteSpace(text) || width <= 1f || lineHeight <= 1f || maxLines <= 0 || y >= bottom)
        {
            return;
        }

        int availableLines = Math.Min(maxLines, Math.Max(0, (int)MathF.Floor((bottom - y) / lineHeight)));

        if (availableLines <= 0)
        {
            return;
        }

        RectangleF layoutRectangle = new(x, y, width, availableLines * lineHeight);

        using StringFormat format = new(StringFormatFlags.LineLimit)
        {
            Trimming = StringTrimming.EllipsisCharacter,
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near
        };

        graphics.DrawString(text, font, brush, layoutRectangle, format);

        SizeF measuredSize = graphics.MeasureString(text, font, new SizeF(width, layoutRectangle.Height), format);
        float consumedHeight = Math.Clamp(measuredSize.Height, lineHeight, layoutRectangle.Height);
        y += consumedHeight;
    }

    /// <summary>
    /// Returns the current visible page of feed items.
    /// </summary>
    /// <remarks>
    /// V0.4.5 can automatically adapt the page density to the available terminal
    /// height. This lets Full HD and larger displays show more RSS entries while
    /// smaller screens remain readable.
    /// </remarks>
    private List<FeedItem> GetCurrentPageItems(RectangleF terminalBounds)
    {
        int itemsPerPage = CalculateItemsPerPage(terminalBounds);

        lock (_itemSync)
        {
            int pageCount = CalculatePageCountUnsafe(itemsPerPage);

            if (_pageIndex >= pageCount)
            {
                _pageIndex = 0;
            }

            int startIndex = _pageIndex * itemsPerPage;
            return _items.Skip(startIndex).Take(itemsPerPage).ToList();
        }
    }

    /// <summary>
    /// Calculates how many pages are needed for the current feed item list.
    /// </summary>
    private int CalculatePageCount(Size viewportSize)
    {
        int itemsPerPage = CalculateItemsPerPage(viewportSize);

        lock (_itemSync)
        {
            return CalculatePageCountUnsafe(itemsPerPage);
        }
    }

    /// <summary>
    /// Calculates the current page count. The caller must hold the item synchronization lock.
    /// </summary>
    private int CalculatePageCountUnsafe(int itemsPerPage)
    {
        return Math.Max(1, (int)Math.Ceiling(_items.Count / (double)Math.Max(1, itemsPerPage)));
    }

    /// <summary>
    /// Calculates the active item count for the current viewport size.
    /// </summary>
    private int CalculateItemsPerPage(Size viewportSize)
    {
        return CalculateItemsPerPage(CalculateTerminalBounds(viewportSize));
    }

    /// <summary>
    /// Calculates how many feed items should be placed on one terminal page.
    /// </summary>
    /// <remarks>
    /// The calculation is intentionally conservative. A feed item may need a source
    /// line, up to two title lines, up to two summary lines and separator spacing.
    /// The result is clamped through the user-configurable minimum and maximum values.
    /// </remarks>
    private int CalculateItemsPerPage(RectangleF terminalBounds)
    {
        if (!_autoFillPage)
        {
            return _configuredItemsPerPage;
        }

        float contentHeight = Math.Max(1f, terminalBounds.Height - 154f);
        float bodyFontSize = CalculateBodyFontSize(terminalBounds);
        float bodyLineHeight = bodyFontSize * 1.55f;
        float summaryLineHeight = bodyFontSize * 1.18f;
        float sourceLineHeight = bodyFontSize * 1.35f;
        float separatorAndPadding = 32f;

        float estimatedItemHeight =
            sourceLineHeight +
            bodyLineHeight * 2f +
            summaryLineHeight * 2f +
            separatorAndPadding;

        int calculatedItems = (int)MathF.Floor(contentHeight / Math.Max(1f, estimatedItemHeight));
        return Math.Clamp(calculatedItems, _minItemsPerPage, _maxItemsPerPage);
    }

    /// <summary>
    /// Returns the current terminal status line without exposing mutable state to rendering.
    /// </summary>
    private string GetStatusLineSnapshot()
    {
        lock (_itemSync)
        {
            return _statusLine;
        }
    }

    /// <summary>
    /// Starts the background RSS/Atom refresh once the effect has been initialized.
    /// </summary>
    private void StartFeedRefreshIfNeeded()
    {
        if (_feedRefreshStarted || !_configurationResult.HasEnabledFeeds)
        {
            return;
        }

        _feedRefreshStarted = true;

        _ = Task.Run(async () =>
        {
            try
            {
                AmberFeedRetrievalResult retrievalResult = await AmberFeedRssLoader.LoadAsync(
                    _configurationResult,
                    _feedRefreshCancellation.Token).ConfigureAwait(false);

                if (retrievalResult.Items.Count > 0)
                {
                    ReplaceItems(retrievalResult.Items, retrievalResult.StatusLine);
                    return;
                }

                IReadOnlyList<AmberFeedDisplayItem> cachedItems = AmberFeedCacheService.LoadItems();

                if (cachedItems.Count > 0)
                {
                    ReplaceItems(cachedItems, $"CACHE FALLBACK  //  {cachedItems.Count} ITEMS  //  RSS UNAVAILABLE");
                    return;
                }

                if (_configurationResult.Configuration?.UseDemoItemsWhenOffline != false)
                {
                    ReplaceItems(ConvertFeedItems(CreateDemoItems()), $"DEMO FALLBACK  //  {retrievalResult.StatusLine}");
                    return;
                }

                UpdateStatusLine(retrievalResult.StatusLine);
            }
            catch (OperationCanceledException)
            {
                // The form is closing. No UI update is required.
            }
            catch (Exception ex)
            {
                UpdateStatusLine($"RSS ERROR  //  {ex.GetType().Name}");
            }
        });
    }

    /// <summary>
    /// Replaces the visible feed items and restarts the page animation from the beginning.
    /// </summary>
    private void ReplaceItems(IReadOnlyList<AmberFeedDisplayItem> items, string statusLine)
    {
        lock (_itemSync)
        {
            _items = ConvertDisplayItems(items);
            _statusLine = statusLine;
            _pageIndex = 0;
            _elapsedOnPage = 0f;
            _visibleCharacters = 0;
        }
    }

    /// <summary>
    /// Updates the status line while keeping the existing items visible.
    /// </summary>
    private void UpdateStatusLine(string statusLine)
    {
        lock (_itemSync)
        {
            _statusLine = statusLine;
        }
    }

    /// <summary>
    /// Resolves the configured page duration while keeping the screensaver readable and safe.
    /// </summary>
    private static float ResolvePageDurationSeconds(AmberFeedConfiguration? configuration)
    {
        double configuredValue = configuration?.PageDurationSeconds ?? DefaultPageDurationSeconds;
        return (float)Math.Clamp(configuredValue, 8.0, 120.0);
    }

    /// <summary>
    /// Resolves the configured typewriter speed. Lower values keep feed text visible longer.
    /// </summary>
    private static float ResolveCharacterRevealRate(AmberFeedConfiguration? configuration)
    {
        double configuredValue = configuration?.CharacterRevealRate ?? DefaultCharacterRevealRate;
        return (float)Math.Clamp(configuredValue, 5.0, 120.0);
    }

    /// <summary>
    /// Resolves the configured fixed page density. It is used when automatic filling is disabled.
    /// </summary>
    private static int ResolveItemsPerPage(AmberFeedConfiguration? configuration)
    {
        int configuredValue = configuration?.ItemsPerPage ?? DefaultItemsPerPage;
        return Math.Clamp(configuredValue, 1, 12);
    }

    /// <summary>
    /// Resolves the minimum page density used by automatic filling.
    /// </summary>
    private static int ResolveMinItemsPerPage(AmberFeedConfiguration? configuration)
    {
        int configuredValue = configuration?.MinItemsPerPage ?? DefaultMinItemsPerPage;
        return Math.Clamp(configuredValue, 1, 12);
    }

    /// <summary>
    /// Resolves the maximum page density used by automatic filling.
    /// </summary>
    private static int ResolveMaxItemsPerPage(AmberFeedConfiguration? configuration, int minItemsPerPage)
    {
        int configuredValue = configuration?.MaxItemsPerPageOnScreen ?? DefaultMaxItemsPerPage;
        return Math.Clamp(configuredValue, minItemsPerPage, 12);
    }

    /// <summary>
    /// Converts display records used by the feed layer into the internal render record.
    /// </summary>
    private static List<FeedItem> ConvertDisplayItems(IEnumerable<AmberFeedDisplayItem> items)
    {
        return items
            .Where(item => !string.IsNullOrWhiteSpace(item.Title))
            .Select(item => new FeedItem(item.Source, item.Title, item.Summary))
            .ToList();
    }

    /// <summary>
    /// Converts internal demo records back into display records for shared fallback logic.
    /// </summary>
    private static List<AmberFeedDisplayItem> ConvertFeedItems(IEnumerable<FeedItem> items)
    {
        return items
            .Select(item => new AmberFeedDisplayItem(item.Source, item.Title, item.Summary))
            .ToList();
    }

    /// <summary>
    /// Reveals a text fragment according to the current page typewriter counter.
    /// </summary>
    private static string RevealText(string text, ref int remainingCharacters)
    {
        if (remainingCharacters <= 0)
        {
            return string.Empty;
        }

        int count = Math.Min(text.Length, remainingCharacters);
        remainingCharacters -= count;
        return text[..count];
    }

    /// <summary>
    /// Creates a monospace font with fallbacks that are normally available on Windows.
    /// </summary>
    private static Font CreateMonospaceFont(float size, FontStyle style)
    {
        return new Font("Consolas", size, style, GraphicsUnit.Point);
    }

    /// <summary>
    /// Chooses a readable body font size for the current terminal size.
    /// </summary>
    private static float CalculateBodyFontSize(RectangleF terminalBounds)
    {
        return Math.Clamp(terminalBounds.Height / 42f, 12f, 18f);
    }

    /// <summary>
    /// Applies a very subtle alpha flicker to emulate an old phosphor display.
    /// </summary>
    private Color ColorWithFlicker(Color color, int baseAlpha)
    {
        int alpha = Math.Clamp((int)(baseAlpha + _flicker * 18f), 0, 255);
        return Color.FromArgb(alpha, color.R, color.G, color.B);
    }

    /// <summary>
    /// Returns a random float in the given range.
    /// </summary>
    private float RandomRange(float minimum, float maximum)
    {
        return minimum + (float)_random.NextDouble() * (maximum - minimum);
    }

    /// <summary>
    /// Creates built-in demo items used when no usable configuration source is available.
    /// </summary>
    private static List<FeedItem> CreateDemoItems()
    {
        return
        [
            new("SASD", "ScreenSaver Lab reaches Amber Feed prototype", "Retro terminal rendering is available. Real RSS loading will follow after cache and timeout handling are designed."),
            new("SECURITY", "Patch window scheduled for laboratory systems", "Demo message showing how operational notices could be displayed on an always-on information screen."),
            new("RESEARCH", "Open datasets queued for review", "A future SASD research dashboard could rotate public science feeds, papers, releases and project notes."),
            new("LINUX", "Kernel and distribution news placeholder", "RSS sources are now read from config/feeds.json; real feed downloads will follow in a later iteration."),
            new("DEV", "Data Stream and Light Trails remain available", "Use /effect:data-stream or /effect:light-trails to switch back to the earlier visual effects."),
            new("OPS", "Power management options are opt-in", "Use /keep-awake or /keep-display-awake only for demos, dashboards and kiosk-like use cases."),
            new("ROADMAP", "Feed cache planned for next iteration", "The screensaver should keep rendering even if a feed is slow, offline or temporarily malformed."),
            new("CONFIG", "Amber theme will later become configurable", "Planned themes include amber, green, white and blue terminal palettes with different scanline strengths.")
        ];
    }

    /// <summary>
    /// Small immutable feed item rendered by the effect.
    /// </summary>
    private sealed record FeedItem(string Source, string Title, string Summary);
}
