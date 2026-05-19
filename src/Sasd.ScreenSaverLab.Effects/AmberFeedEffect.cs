using System.Drawing;
using System.Drawing.Drawing2D;
using Sasd.ScreenSaverLab.Core;
using Sasd.ScreenSaverLab.Effects.Feeds;

namespace Sasd.ScreenSaverLab.Effects;

/// <summary>
/// Displays rotating feed-style items in a warm amber retro-terminal style.
/// </summary>
/// <remarks>
/// Amber Feed is the first stage of the planned RSS-capable screensaver effect. V0.4.1
/// can load and validate <c>config/feeds.json</c>, but deliberately still avoids live
/// network access. Real RSS retrieval, caching and timeout handling are planned for the
/// next iterations.
/// </remarks>
public sealed class AmberFeedEffect : IScreenSaverEffect
{
    private const float PageDurationSeconds = 11.5f;
    private const float CharacterRevealRate = 46f;
    private const int ItemsPerPage = 4;

    private static readonly Color BackgroundColor = Color.FromArgb(255, 3, 2, 0);
    private static readonly Color DarkAmberColor = Color.FromArgb(255, 82, 47, 0);
    private static readonly Color AmberColor = Color.FromArgb(255, 255, 176, 40);
    private static readonly Color BrightAmberColor = Color.FromArgb(255, 255, 223, 128);
    private static readonly Color DimAmberColor = Color.FromArgb(255, 155, 91, 12);

    private readonly Random _random = new();
    private readonly List<FeedItem> _items;
    private readonly string _statusLine;

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

        _items = configurationResult.Items.Count > 0
            ? configurationResult.Items.Select(item => new FeedItem(item.Source, item.Title, item.Summary)).ToList()
            : CreateDemoItems();

        _statusLine = configurationResult.StatusLine;
    }

    /// <inheritdoc />
    public string Name => "Amber Feed";

    /// <inheritdoc />
    public string Description => "Amber retro terminal feed display with configured RSS source preview and teletext-like motion.";

    /// <inheritdoc />
    public void Initialize(Size viewportSize)
    {
        _elapsedOnPage = 0f;
        _totalElapsed = 0f;
        _pageIndex = 0;
        _visibleCharacters = 0;
        _flicker = 0f;
    }

    /// <inheritdoc />
    public void Update(TimeSpan elapsed, Size viewportSize)
    {
        float seconds = MathF.Min((float)elapsed.TotalSeconds, 0.12f);

        _elapsedOnPage += seconds;
        _totalElapsed += seconds;
        _flicker = 0.5f + RandomRange(-0.08f, 0.08f);

        if (_elapsedOnPage >= PageDurationSeconds)
        {
            _elapsedOnPage = 0f;
            _visibleCharacters = 0;
            _pageIndex = (_pageIndex + 1) % CalculatePageCount();
        }

        _visibleCharacters = Math.Max(_visibleCharacters, (int)(_elapsedOnPage * CharacterRevealRate));
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
        string status = $"{_statusLine}  //  {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        graphics.DrawString(header, headerFont, brightBrush, terminalBounds.X + 28f, terminalBounds.Y + 20f);
        graphics.DrawString(status, footerFont, dimBrush, terminalBounds.X + 28f, terminalBounds.Bottom - 31f);

        List<FeedItem> pageItems = GetCurrentPageItems();
        float y = terminalBounds.Y + 78f;
        float lineHeight = bodyFont.GetHeight(graphics) + 5f;
        int remainingCharacters = _visibleCharacters;

        foreach (FeedItem item in pageItems)
        {
            string sourceLine = $"[{item.Source}]";
            string titleLine = item.Title.ToUpperInvariant();
            string summaryLine = item.Summary;

            if (remainingCharacters <= 0)
            {
                break;
            }

            string visibleSource = RevealText(sourceLine, ref remainingCharacters);
            graphics.DrawString(visibleSource, sourceFont, brightBrush, terminalBounds.X + 30f, y);
            y += lineHeight * 0.9f;

            string visibleTitle = RevealText(titleLine, ref remainingCharacters);
            DrawWrappedText(graphics, visibleTitle, bodyFont, amberBrush, terminalBounds.X + 30f, ref y, terminalBounds.Width - 60f, lineHeight, maxLines: 2);

            string visibleSummary = RevealText(summaryLine, ref remainingCharacters);
            DrawWrappedText(graphics, visibleSummary, footerFont, dimBrush, terminalBounds.X + 30f, ref y, terminalBounds.Width - 60f, footerFont.GetHeight(graphics) + 4f, maxLines: 2);

            y += 16f;

            using Pen separatorPen = new(Color.FromArgb(74, 255, 176, 40), 1f);
            graphics.DrawLine(separatorPen, terminalBounds.X + 30f, y, terminalBounds.Right - 30f, y);
            y += 16f;
        }

        DrawPageProgress(graphics, terminalBounds, darkBrush, amberBrush);
    }

    /// <summary>
    /// Draws a small animated progress marker near the bottom of the terminal.
    /// </summary>
    private void DrawPageProgress(Graphics graphics, RectangleF terminalBounds, Brush dimBrush, Brush amberBrush)
    {
        float progress = Math.Clamp(_elapsedOnPage / PageDurationSeconds, 0f, 1f);
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
    /// Draws wrapped text with a simple fixed-width approximation.
    /// </summary>
    private static void DrawWrappedText(
        Graphics graphics,
        string text,
        Font font,
        Brush brush,
        float x,
        ref float y,
        float width,
        float lineHeight,
        int maxLines)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        int approximateCharactersPerLine = Math.Max(18, (int)(width / Math.Max(7f, font.Size * 0.66f)));
        string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        List<string> lines = [];
        string currentLine = string.Empty;

        foreach (string word in words)
        {
            string candidate = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;

            if (candidate.Length > approximateCharactersPerLine && currentLine.Length > 0)
            {
                lines.Add(currentLine);
                currentLine = word;
            }
            else
            {
                currentLine = candidate;
            }

            if (lines.Count >= maxLines)
            {
                break;
            }
        }

        if (lines.Count < maxLines && currentLine.Length > 0)
        {
            lines.Add(currentLine);
        }

        foreach (string line in lines.Take(maxLines))
        {
            graphics.DrawString(line, font, brush, x, y);
            y += lineHeight;
        }
    }

    /// <summary>
    /// Returns the current visible page of demo feed items.
    /// </summary>
    private List<FeedItem> GetCurrentPageItems()
    {
        int startIndex = _pageIndex * ItemsPerPage;
        return _items.Skip(startIndex).Take(ItemsPerPage).ToList();
    }

    /// <summary>
    /// Calculates how many pages are needed for the built-in demo items.
    /// </summary>
    private int CalculatePageCount()
    {
        return Math.Max(1, (int)Math.Ceiling(_items.Count / (double)ItemsPerPage));
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
