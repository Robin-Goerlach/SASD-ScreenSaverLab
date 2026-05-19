using System.Drawing;
using System.Drawing.Drawing2D;
using Sasd.ScreenSaverLab.Core;

namespace Sasd.ScreenSaverLab.Effects;

/// <summary>
/// A calm code-rain inspired effect with falling glyph streams.
/// </summary>
/// <remarks>
/// This effect is deliberately an original SASD-style implementation. It uses simple
/// ASCII-like technical glyphs instead of copying a movie or vendor-specific look. The
/// goal for V0.2 is to prove that several built-in effects can live behind the same host
/// interface and can be selected by command-line option.
/// </remarks>
public sealed class DigitalRainEffect : IScreenSaverEffect
{
    private const int CellWidth = 18;
    private const int CellHeight = 20;
    private const int MinimumColumnCount = 20;
    private const int MaximumColumnCount = 220;

    private static readonly char[] Glyphs =
    [
        '0', '1', '2', '3', '5', '8',
        'S', 'A', 'D',
        '<', '>', '/', '\\', '*', '+', '-', '=',
        '[', ']', '{', '}', '(', ')', ':', ';'
    ];

    private readonly Random _random = new();
    private readonly List<RainColumn> _columns = [];
    private float _animationTime;

    /// <inheritdoc />
    public string Name => "Digital Rain";

    /// <inheritdoc />
    public string Description => "A SASD code-rain effect with falling technical glyph streams.";

    /// <inheritdoc />
    public void Initialize(Size viewportSize)
    {
        _animationTime = 0;
        _columns.Clear();

        if (viewportSize.Width <= 0 || viewportSize.Height <= 0)
        {
            return;
        }

        int columnCount = CalculateColumnCount(viewportSize);

        for (int index = 0; index < columnCount; index++)
        {
            float x = index * CellWidth + RandomRange(-2f, 2f);
            _columns.Add(CreateColumn(x, viewportSize, randomizeVerticalPosition: true));
        }
    }

    /// <inheritdoc />
    public void Update(TimeSpan elapsed, Size viewportSize)
    {
        if (viewportSize.Width <= 0 || viewportSize.Height <= 0)
        {
            return;
        }

        float seconds = (float)elapsed.TotalSeconds;

        // Avoid huge jumps after a debugger pause or a temporary system stall.
        seconds = MathF.Min(seconds, 0.12f);
        _animationTime += seconds;

        for (int index = 0; index < _columns.Count; index++)
        {
            RainColumn column = _columns[index];
            column.HeadY += column.Speed * seconds;
            column.GlyphPhase += column.GlyphChangeSpeed * seconds;

            float resetY = viewportSize.Height + column.TrailLength * CellHeight + 80;

            if (column.HeadY > resetY)
            {
                column = CreateColumn(column.X, viewportSize, randomizeVerticalPosition: false);
            }

            _columns[index] = column;
        }
    }

    /// <inheritdoc />
    public void Render(Graphics graphics, Size viewportSize)
    {
        if (viewportSize.Width <= 0 || viewportSize.Height <= 0)
        {
            return;
        }

        graphics.SmoothingMode = SmoothingMode.None;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
        graphics.CompositingQuality = CompositingQuality.HighSpeed;

        DrawBackground(graphics, viewportSize);
        DrawColumns(graphics, viewportSize);
    }

    /// <summary>
    /// Calculates a useful column count for the current viewport.
    /// </summary>
    private static int CalculateColumnCount(Size viewportSize)
    {
        int calculated = viewportSize.Width / CellWidth + 3;
        return Math.Clamp(calculated, MinimumColumnCount, MaximumColumnCount);
    }

    /// <summary>
    /// Creates randomized movement state for one rain column.
    /// </summary>
    private RainColumn CreateColumn(float x, Size viewportSize, bool randomizeVerticalPosition)
    {
        int trailLength = _random.Next(10, 34);

        return new RainColumn
        {
            X = x,
            HeadY = randomizeVerticalPosition
                ? RandomRange(-viewportSize.Height, viewportSize.Height)
                : RandomRange(-trailLength * CellHeight - viewportSize.Height * 0.35f, -CellHeight),
            Speed = RandomRange(75f, 245f),
            TrailLength = trailLength,
            Seed = _random.Next(0, 100_000),
            GlyphPhase = RandomRange(0f, 10f),
            GlyphChangeSpeed = RandomRange(4f, 11f),
            Brightness = RandomRange(0.72f, 1.0f)
        };
    }

    /// <summary>
    /// Draws a dark technical background with a slight blue-green gradient.
    /// </summary>
    private static void DrawBackground(Graphics graphics, Size viewportSize)
    {
        Rectangle bounds = new(0, 0, viewportSize.Width, viewportSize.Height);

        using LinearGradientBrush brush = new(
            bounds,
            Color.FromArgb(255, 0, 8, 11),
            Color.FromArgb(255, 0, 22, 27),
            LinearGradientMode.Vertical);

        graphics.FillRectangle(brush, bounds);

        // A subtle translucent veil prevents the rain from looking too harsh on large
        // bright monitors and gives the effect a calmer screensaver character.
        using SolidBrush veil = new(Color.FromArgb(45, 0, 0, 0));
        graphics.FillRectangle(veil, bounds);
    }

    /// <summary>
    /// Draws every falling column.
    /// </summary>
    private void DrawColumns(Graphics graphics, Size viewportSize)
    {
        using Font glyphFont = new("Consolas", 17f, FontStyle.Bold, GraphicsUnit.Pixel);
        using StringFormat centeredFormat = new()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        foreach (RainColumn column in _columns)
        {
            DrawColumn(graphics, glyphFont, centeredFormat, column, viewportSize);
        }
    }

    /// <summary>
    /// Draws a single column head and its fading trail.
    /// </summary>
    private void DrawColumn(
        Graphics graphics,
        Font glyphFont,
        StringFormat centeredFormat,
        RainColumn column,
        Size viewportSize)
    {
        for (int trailIndex = 0; trailIndex < column.TrailLength; trailIndex++)
        {
            float y = column.HeadY - trailIndex * CellHeight;

            if (y < -CellHeight || y > viewportSize.Height + CellHeight)
            {
                continue;
            }

            float fade = 1f - trailIndex / (float)column.TrailLength;
            fade = MathF.Pow(fade, 1.6f);

            bool isHead = trailIndex == 0;
            int alpha = isHead
                ? 235
                : Math.Clamp((int)(210f * fade * column.Brightness), 18, 200);

            Color glyphColor = isHead
                ? Color.FromArgb(alpha, 225, 255, 245)
                : Color.FromArgb(alpha, 80, 225, 190);

            char glyph = PickGlyph(column, trailIndex);
            RectangleF cell = new(column.X, y, CellWidth, CellHeight);

            using SolidBrush brush = new(glyphColor);
            graphics.DrawString(glyph.ToString(), glyphFont, brush, cell, centeredFormat);
        }
    }

    /// <summary>
    /// Selects a pseudo-random glyph that changes over time but remains stable enough
    /// that the rain does not flicker aggressively.
    /// </summary>
    private char PickGlyph(RainColumn column, int trailIndex)
    {
        int phase = (int)(column.GlyphPhase + _animationTime * column.GlyphChangeSpeed);
        int index = Math.Abs(column.Seed + trailIndex * 13 + phase) % Glyphs.Length;
        return Glyphs[index];
    }

    /// <summary>
    /// Returns a random single-precision value in the requested interval.
    /// </summary>
    private float RandomRange(float minInclusive, float maxExclusive)
    {
        return minInclusive + (float)_random.NextDouble() * (maxExclusive - minInclusive);
    }

    /// <summary>
    /// Mutable state for one falling glyph column.
    /// </summary>
    private struct RainColumn
    {
        public float X;
        public float HeadY;
        public float Speed;
        public int TrailLength;
        public int Seed;
        public float GlyphPhase;
        public float GlyphChangeSpeed;
        public float Brightness;
    }
}
