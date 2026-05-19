using System.Drawing;
using System.Drawing.Drawing2D;
using Sasd.ScreenSaverLab.Core;

namespace Sasd.ScreenSaverLab.Effects;

/// <summary>
/// Draws an original 1980s-inspired wireframe landscape with perspective grid motion.
/// </summary>
/// <remarks>
/// The effect intentionally uses simple vector-like lines instead of bitmap assets. It is
/// inspired by old terminal, CAD and demo-scene visuals, but it does not copy a specific
/// historic screensaver or game scene. The implementation stays GDI+-friendly so it can
/// run inside the current Windows Forms host without an OpenGL or shader dependency.
/// </remarks>
public sealed class WireframeTerrainEffect : IScreenSaverEffect
{
    private const int HorizonRows = 34;
    private const int VerticalGridLines = 31;
    private const float RowSpacing = 0.115f;
    private const float RoadHalfWidth = 0.82f;

    private float _time;

    /// <inheritdoc />
    public string Name => "Wireframe Terrain";

    /// <inheritdoc />
    public string Description => "Retro vector landscape with moving perspective grid lines.";

    /// <inheritdoc />
    public void Initialize(Size viewportSize)
    {
        _time = 0f;
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

        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.CompositingQuality = CompositingQuality.HighQuality;

        DrawBackground(graphics, viewportSize);
        DrawSun(graphics, viewportSize);
        DrawMountains(graphics, viewportSize);
        DrawTerrainGrid(graphics, viewportSize);
        DrawHorizonGlow(graphics, viewportSize);
    }

    /// <summary>
    /// Draws the dark blue/purple sky and near-black ground base.
    /// </summary>
    private static void DrawBackground(Graphics graphics, Size viewportSize)
    {
        Rectangle bounds = new(0, 0, viewportSize.Width, viewportSize.Height);
        float horizonY = GetHorizonY(viewportSize);

        using LinearGradientBrush sky = new(
            new Rectangle(0, 0, viewportSize.Width, Math.Max(1, (int)horizonY + 24)),
            Color.FromArgb(255, 4, 6, 22),
            Color.FromArgb(255, 25, 9, 45),
            LinearGradientMode.Vertical);
        graphics.FillRectangle(sky, 0, 0, viewportSize.Width, horizonY + 24);

        using LinearGradientBrush ground = new(
            new Rectangle(0, (int)horizonY, viewportSize.Width, Math.Max(1, viewportSize.Height - (int)horizonY)),
            Color.FromArgb(255, 5, 4, 12),
            Color.FromArgb(255, 0, 0, 3),
            LinearGradientMode.Vertical);
        graphics.FillRectangle(ground, 0, horizonY, viewportSize.Width, viewportSize.Height - horizonY);

        using SolidBrush veil = new(Color.FromArgb(30, 0, 0, 0));
        graphics.FillRectangle(veil, bounds);
    }

    /// <summary>
    /// Draws a simple striped low sun behind the wireframe horizon.
    /// </summary>
    private static void DrawSun(Graphics graphics, Size viewportSize)
    {
        float horizonY = GetHorizonY(viewportSize);
        float radius = Math.Min(viewportSize.Width, viewportSize.Height) * 0.115f;
        PointF center = new(viewportSize.Width * 0.78f, horizonY - radius * 0.34f);
        RectangleF sunBounds = new(center.X - radius, center.Y - radius, radius * 2f, radius * 2f);

        using GraphicsPath sunPath = new();
        sunPath.AddEllipse(sunBounds);

        using PathGradientBrush glow = new(sunPath)
        {
            CenterColor = Color.FromArgb(160, 255, 150, 72),
            SurroundColors = [Color.FromArgb(0, 255, 90, 25)]
        };
        graphics.FillPath(glow, sunPath);

        using Pen sunPen = new(Color.FromArgb(130, 255, 165, 72), 1.5f);
        graphics.DrawEllipse(sunPen, sunBounds);

        using Pen stripePen = new(Color.FromArgb(95, 6, 5, 24), Math.Max(2f, radius * 0.035f));
        for (float y = sunBounds.Top + radius * 0.48f; y < sunBounds.Bottom; y += Math.Max(6f, radius * 0.18f))
        {
            graphics.DrawLine(stripePen, sunBounds.Left, y, sunBounds.Right, y);
        }
    }

    /// <summary>
    /// Draws a slowly moving low-poly mountain silhouette behind the horizon.
    /// </summary>
    private void DrawMountains(Graphics graphics, Size viewportSize)
    {
        float horizonY = GetHorizonY(viewportSize);
        using Pen backPen = new(Color.FromArgb(70, 90, 220, 255), 1.2f);
        using Pen frontPen = new(Color.FromArgb(115, 255, 82, 190), 1.4f);

        DrawMountainLayer(graphics, viewportSize, horizonY, backPen, phase: _time * 0.08f, amplitude: 0.105f, baselineOffset: 0.026f);
        DrawMountainLayer(graphics, viewportSize, horizonY, frontPen, phase: _time * 0.13f + 1.7f, amplitude: 0.075f, baselineOffset: 0.008f);
    }

    /// <summary>
    /// Draws one jagged mountain polyline.
    /// </summary>
    private static void DrawMountainLayer(
        Graphics graphics,
        Size viewportSize,
        float horizonY,
        Pen pen,
        float phase,
        float amplitude,
        float baselineOffset)
    {
        const int pointCount = 20;
        PointF[] points = new PointF[pointCount];

        for (int index = 0; index < pointCount; index++)
        {
            float t = index / (float)(pointCount - 1);
            float wave = MathF.Sin(t * 10.5f + phase) * 0.55f + MathF.Sin(t * 22.0f - phase * 1.6f) * 0.45f;
            float y = horizonY - viewportSize.Height * (baselineOffset + amplitude * (0.45f + MathF.Abs(wave) * 0.55f));
            points[index] = new PointF(t * viewportSize.Width, y);
        }

        graphics.DrawLines(pen, points);
    }

    /// <summary>
    /// Draws the perspective road/grid. Horizontal lines use a non-linear mapping so
    /// spacing becomes larger near the viewer, creating depth without 3D dependencies.
    /// </summary>
    private void DrawTerrainGrid(Graphics graphics, Size viewportSize)
    {
        float horizonY = GetHorizonY(viewportSize);
        float centerX = viewportSize.Width * 0.5f;
        float groundHeight = viewportSize.Height - horizonY;
        float timeOffset = (_time * 0.72f) % RowSpacing;

        using Pen cyanPen = new(Color.FromArgb(125, 70, 235, 255), 1.1f);
        using Pen magentaPen = new(Color.FromArgb(110, 255, 72, 210), 1.0f);
        using Pen brightPen = new(Color.FromArgb(180, 160, 250, 255), 1.4f);

        // Horizontal grid rows.
        for (int row = 0; row < HorizonRows; row++)
        {
            float depth = row * RowSpacing + timeOffset;
            float perspective = 1f - 1f / (1f + depth * depth * 1.45f);
            float y = horizonY + perspective * groundHeight;

            if (y > viewportSize.Height + 10)
            {
                continue;
            }

            float halfWidth = RoadHalfWidth * viewportSize.Width * (0.04f + perspective * 1.22f);
            float cameraSway = MathF.Sin(_time * 0.45f + perspective * 2.8f) * viewportSize.Width * 0.035f * perspective;
            PointF left = new(centerX - halfWidth + cameraSway, y);
            PointF right = new(centerX + halfWidth + cameraSway, y);

            Pen rowPen = row % 5 == 0 ? brightPen : cyanPen;
            graphics.DrawLine(rowPen, left, right);

            // Draw a very light highlight at the nearest row, making motion easier to see.
            if (row == HorizonRows - 1)
            {
                using Pen nearPen = new(Color.FromArgb(80, 255, 255, 255), 2.0f);
                graphics.DrawLine(nearPen, left, right);
            }

        }

        // Vanishing lines.
        for (int index = 0; index < VerticalGridLines; index++)
        {
            float n = index / (float)(VerticalGridLines - 1);
            float normalizedX = (n - 0.5f) * 2f;
            float bend = MathF.Sin(_time * 0.32f + normalizedX * 1.8f) * 0.025f;
            float bottomX = centerX + (normalizedX + bend) * viewportSize.Width * RoadHalfWidth;
            float horizonX = centerX + MathF.Sin(_time * 0.45f) * viewportSize.Width * 0.025f;

            Pen pen = Math.Abs(index - VerticalGridLines / 2) <= 1 ? brightPen : magentaPen;
            graphics.DrawLine(pen, horizonX, horizonY, bottomX, viewportSize.Height + 8);
        }
    }

    /// <summary>
    /// Draws a subtle glow line at the horizon so the composition reads from a distance.
    /// </summary>
    private static void DrawHorizonGlow(Graphics graphics, Size viewportSize)
    {
        float horizonY = GetHorizonY(viewportSize);

        using Pen glowWide = new(Color.FromArgb(42, 110, 230, 255), 9f);
        using Pen glowThin = new(Color.FromArgb(160, 120, 245, 255), 1.6f);
        graphics.DrawLine(glowWide, 0, horizonY, viewportSize.Width, horizonY);
        graphics.DrawLine(glowThin, 0, horizonY, viewportSize.Width, horizonY);
    }

    /// <summary>
    /// Returns the perspective horizon for the current viewport.
    /// </summary>
    private static float GetHorizonY(Size viewportSize)
    {
        return viewportSize.Height * 0.43f;
    }
}
