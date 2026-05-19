using System.Drawing;
using System.Drawing.Drawing2D;
using Sasd.ScreenSaverLab.Core;

namespace Sasd.ScreenSaverLab.Effects;

/// <summary>
/// Draws a colorful low-resolution plasma field combined with a subtle technical grid.
/// </summary>
/// <remarks>
/// This is a small GDI+-friendly nod to classic demo-scene plasma effects. It renders a
/// deliberately low-resolution buffer and scales it up, which keeps the effect fast and
/// gives it a slightly retro pixel glow. The grid overlay makes it fit the SASD visual
/// language instead of becoming a generic color wash.
/// </remarks>
public sealed class PlasmaGridEffect : IScreenSaverEffect
{
    private const int BufferWidth = 180;
    private const int BufferHeight = 112;
    private const int GridSpacing = 54;

    private Bitmap? _buffer;
    private float _time;

    /// <inheritdoc />
    public string Name => "Plasma Grid";

    /// <inheritdoc />
    public string Description => "Retro demo-scene plasma with a calm technical grid overlay.";

    /// <inheritdoc />
    public void Initialize(Size viewportSize)
    {
        _time = 0f;
        EnsureBuffer();
    }

    /// <inheritdoc />
    public void Update(TimeSpan elapsed, Size viewportSize)
    {
        float seconds = (float)Math.Min(elapsed.TotalSeconds, 0.12);
        _time += seconds;
    }

    /// <inheritdoc />
    public void Render(Graphics graphics, Size viewportSize)
    {
        if (viewportSize.Width <= 0 || viewportSize.Height <= 0)
        {
            return;
        }

        EnsureBuffer();
        RenderPlasmaBuffer();

        graphics.SmoothingMode = SmoothingMode.None;
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = PixelOffsetMode.Half;

        Rectangle target = new(0, 0, viewportSize.Width, viewportSize.Height);
        graphics.DrawImage(_buffer!, target);

        DrawVignette(graphics, viewportSize);
        DrawGrid(graphics, viewportSize);
        DrawPulseRings(graphics, viewportSize);
    }

    /// <summary>
    /// Creates the reusable low-resolution render buffer.
    /// </summary>
    private void EnsureBuffer()
    {
        if (_buffer is { Width: BufferWidth, Height: BufferHeight })
        {
            return;
        }

        _buffer?.Dispose();
        _buffer = new Bitmap(BufferWidth, BufferHeight);
    }

    /// <summary>
    /// Fills the low-resolution plasma buffer.
    /// </summary>
    private void RenderPlasmaBuffer()
    {
        if (_buffer is null)
        {
            return;
        }

        float time = _time * 0.82f;

        for (int y = 0; y < BufferHeight; y++)
        {
            float ny = (y - BufferHeight * 0.5f) / BufferHeight;

            for (int x = 0; x < BufferWidth; x++)
            {
                float nx = (x - BufferWidth * 0.5f) / BufferWidth;
                float radius = MathF.Sqrt(nx * nx + ny * ny);
                float angle = MathF.Atan2(ny, nx);

                float value = 0f;
                value += MathF.Sin(nx * 18.0f + time * 1.70f);
                value += MathF.Sin(ny * 16.0f - time * 1.30f);
                value += MathF.Sin((nx + ny) * 13.5f + time * 1.10f);
                value += MathF.Sin(radius * 34.0f - time * 2.25f + MathF.Sin(angle * 3f + time));
                value *= 0.25f;

                float normalized = (value + 1f) * 0.5f;
                _buffer.SetPixel(x, y, ColorFromPlasma(normalized, radius));
            }
        }
    }

    /// <summary>
    /// Maps a plasma value to a restrained SASD-like palette.
    /// </summary>
    private static Color ColorFromPlasma(float value, float radius)
    {
        value = Math.Clamp(value, 0f, 1f);
        float edgeFade = Math.Clamp(1f - radius * 1.35f, 0.22f, 1f);

        int r = (int)((18 + 115 * MathF.Pow(value, 1.8f) + 40 * MathF.Sin(value * MathF.PI)) * edgeFade);
        int g = (int)((32 + 125 * value + 45 * (1f - value)) * edgeFade);
        int b = (int)((55 + 150 * (1f - value) + 35 * MathF.Sin(value * MathF.PI)) * edgeFade);

        return Color.FromArgb(
            Math.Clamp(r, 0, 255),
            Math.Clamp(g, 0, 255),
            Math.Clamp(b, 0, 255));
    }

    /// <summary>
    /// Darkens the corners so the effect remains comfortable as a screensaver.
    /// </summary>
    private static void DrawVignette(Graphics graphics, Size viewportSize)
    {
        using GraphicsPath path = new();
        path.AddEllipse(
            -viewportSize.Width * 0.18f,
            -viewportSize.Height * 0.28f,
            viewportSize.Width * 1.36f,
            viewportSize.Height * 1.56f);

        using PathGradientBrush brush = new(path)
        {
            CenterColor = Color.FromArgb(0, 0, 0, 0),
            SurroundColors = [Color.FromArgb(165, 0, 0, 0)]
        };

        graphics.FillRectangle(brush, 0, 0, viewportSize.Width, viewportSize.Height);
    }

    /// <summary>
    /// Draws a soft technical grid over the plasma field.
    /// </summary>
    private void DrawGrid(Graphics graphics, Size viewportSize)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        int offset = (int)((_time * 18f) % GridSpacing);
        using Pen gridPen = new(Color.FromArgb(48, 190, 245, 255), 1f);
        using Pen majorPen = new(Color.FromArgb(82, 255, 210, 255), 1.25f);

        for (int x = -GridSpacing + offset; x < viewportSize.Width + GridSpacing; x += GridSpacing)
        {
            Pen pen = Math.Abs(x - viewportSize.Width / 2) < GridSpacing / 2 ? majorPen : gridPen;
            graphics.DrawLine(pen, x, 0, x, viewportSize.Height);
        }

        for (int y = -GridSpacing + offset; y < viewportSize.Height + GridSpacing; y += GridSpacing)
        {
            Pen pen = Math.Abs(y - viewportSize.Height / 2) < GridSpacing / 2 ? majorPen : gridPen;
            graphics.DrawLine(pen, 0, y, viewportSize.Width, y);
        }
    }

    /// <summary>
    /// Adds slow orbit-like rings to give the plasma a composed center.
    /// </summary>
    private void DrawPulseRings(Graphics graphics, Size viewportSize)
    {
        PointF center = new(viewportSize.Width * 0.5f, viewportSize.Height * 0.5f);
        float baseRadius = Math.Min(viewportSize.Width, viewportSize.Height) * 0.13f;

        for (int index = 0; index < 5; index++)
        {
            float pulse = (MathF.Sin(_time * 0.9f + index * 0.75f) + 1f) * 0.5f;
            float radius = baseRadius * (1.0f + index * 0.55f + pulse * 0.18f);
            int alpha = Math.Clamp(88 - index * 13, 18, 90);

            using Pen pen = new(Color.FromArgb(alpha, 230, 245, 255), 1.2f);
            graphics.DrawEllipse(pen, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
        }
    }
}
