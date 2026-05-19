using System.Drawing;
using System.Drawing.Drawing2D;
using Sasd.ScreenSaverLab.Core;

namespace Sasd.ScreenSaverLab.Effects;

/// <summary>
/// A calm visualizer-style effect with softly glowing moving light trails.
/// </summary>
/// <remarks>
/// Light Trails is an original SASD effect inspired by the general idea of flowing
/// fullscreen visualizers, not by a specific commercial screensaver. The implementation
/// deliberately uses simple GDI+ drawing primitives so it remains easy to understand
/// before the project later experiments with SkiaSharp, OpenGL or shader-based rendering.
/// </remarks>
public sealed class LightTrailsEffect : IScreenSaverEffect
{
    private const int MinimumTrailCount = 8;
    private const int MaximumTrailCount = 30;
    private const int MaximumHistoryPoints = 44;

    private readonly Random _random = new();
    private readonly List<LightTrail> _trails = [];
    private float _animationTime;

    /// <inheritdoc />
    public string Name => "Light Trails";

    /// <inheritdoc />
    public string Description => "Soft glowing SASD light trails moving across a dark visualizer field.";

    /// <inheritdoc />
    public void Initialize(Size viewportSize)
    {
        _animationTime = 0;
        _trails.Clear();

        if (viewportSize.Width <= 0 || viewportSize.Height <= 0)
        {
            return;
        }

        int trailCount = CalculateTrailCount(viewportSize);

        for (int i = 0; i < trailCount; i++)
        {
            _trails.Add(CreateTrail(viewportSize, i));
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

        // Clamp unusually large frame gaps, for example after a debugger break.
        seconds = MathF.Min(seconds, 0.12f);
        _animationTime += seconds;

        foreach (LightTrail trail in _trails)
        {
            UpdateTrail(trail, seconds, viewportSize);
        }
    }

    /// <inheritdoc />
    public void Render(Graphics graphics, Size viewportSize)
    {
        if (viewportSize.Width <= 0 || viewportSize.Height <= 0)
        {
            return;
        }

        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.CompositingQuality = CompositingQuality.HighQuality;

        DrawBackground(graphics, viewportSize);
        DrawTrails(graphics);
        DrawTrailHeads(graphics);
    }

    /// <summary>
    /// Calculates a useful trail count based on the visible screen area.
    /// </summary>
    private static int CalculateTrailCount(Size viewportSize)
    {
        double megapixels = Math.Max(1.0, viewportSize.Width * viewportSize.Height / 1_000_000.0);
        int calculated = (int)(12 * megapixels);

        return Math.Clamp(calculated, MinimumTrailCount, MaximumTrailCount);
    }

    /// <summary>
    /// Creates one moving light trail with randomized position, speed and color family.
    /// </summary>
    private LightTrail CreateTrail(Size viewportSize, int index)
    {
        PointF position = new(
            RandomRange(0, Math.Max(1, viewportSize.Width)),
            RandomRange(0, Math.Max(1, viewportSize.Height)));

        float angle = RandomRange(0, MathF.PI * 2f);
        float speed = RandomRange(28f, 92f);

        LightTrail trail = new()
        {
            Position = position,
            Velocity = new PointF(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed),
            BaseSpeed = speed,
            Phase = RandomRange(0, MathF.PI * 2f),
            WaveSpeed = RandomRange(0.55f, 1.75f),
            WaveStrength = RandomRange(10f, 34f),
            Thickness = RandomRange(1.3f, 3.3f),
            Brightness = RandomRange(0.65f, 1.0f),
            ColorFamily = index % 4
        };

        // Seed the history with the current position so the first frame already has
        // a small visible point instead of waiting several frames for a trail to grow.
        trail.History.Add(position);
        return trail;
    }

    /// <summary>
    /// Advances one light trail and keeps it inside the viewport by gently bouncing it.
    /// </summary>
    private void UpdateTrail(LightTrail trail, float seconds, Size viewportSize)
    {
        float wave = MathF.Sin(_animationTime * trail.WaveSpeed + trail.Phase);
        float sideWave = MathF.Cos(_animationTime * trail.WaveSpeed * 0.73f + trail.Phase * 1.7f);

        // Add a subtle curved movement so the paths feel organic rather than like
        // straight billiard balls crossing the screen.
        trail.Velocity = new PointF(
            trail.Velocity.X + wave * trail.WaveStrength * seconds,
            trail.Velocity.Y + sideWave * trail.WaveStrength * seconds);

        trail.Velocity = LimitVelocity(trail.Velocity, trail.BaseSpeed * 0.72f, trail.BaseSpeed * 1.45f);

        trail.Position = new PointF(
            trail.Position.X + trail.Velocity.X * seconds,
            trail.Position.Y + trail.Velocity.Y * seconds);

        BounceAtEdges(trail, viewportSize);
        RememberPosition(trail);
    }

    /// <summary>
    /// Keeps the velocity magnitude between a lower and an upper bound.
    /// </summary>
    private static PointF LimitVelocity(PointF velocity, float minimum, float maximum)
    {
        float length = MathF.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);

        if (length <= 0.001f)
        {
            return new PointF(minimum, 0);
        }

        float targetLength = Math.Clamp(length, minimum, maximum);
        float factor = targetLength / length;

        return new PointF(velocity.X * factor, velocity.Y * factor);
    }

    /// <summary>
    /// Bounces a light trail at screen edges while keeping it inside a small margin.
    /// </summary>
    private static void BounceAtEdges(LightTrail trail, Size viewportSize)
    {
        const float margin = 24f;
        float x = trail.Position.X;
        float y = trail.Position.Y;
        float vx = trail.Velocity.X;
        float vy = trail.Velocity.Y;

        if (x < margin)
        {
            x = margin;
            vx = Math.Abs(vx);
        }
        else if (x > viewportSize.Width - margin)
        {
            x = viewportSize.Width - margin;
            vx = -Math.Abs(vx);
        }

        if (y < margin)
        {
            y = margin;
            vy = Math.Abs(vy);
        }
        else if (y > viewportSize.Height - margin)
        {
            y = viewportSize.Height - margin;
            vy = -Math.Abs(vy);
        }

        trail.Position = new PointF(x, y);
        trail.Velocity = new PointF(vx, vy);
    }

    /// <summary>
    /// Adds the current position to the trail history and removes old points.
    /// </summary>
    private static void RememberPosition(LightTrail trail)
    {
        trail.History.Add(trail.Position);

        while (trail.History.Count > MaximumHistoryPoints)
        {
            trail.History.RemoveAt(0);
        }
    }

    /// <summary>
    /// Draws a calm dark background with a slight blue/petrol visualizer character.
    /// </summary>
    private static void DrawBackground(Graphics graphics, Size viewportSize)
    {
        Rectangle bounds = new(0, 0, viewportSize.Width, viewportSize.Height);

        using LinearGradientBrush background = new(
            bounds,
            Color.FromArgb(255, 3, 6, 18),
            Color.FromArgb(255, 4, 28, 38),
            LinearGradientMode.ForwardDiagonal);

        graphics.FillRectangle(background, bounds);

        using SolidBrush vignette = new(Color.FromArgb(55, 0, 0, 0));
        graphics.FillRectangle(vignette, bounds);
    }

    /// <summary>
    /// Draws all trail histories as layered translucent line segments.
    /// </summary>
    private void DrawTrails(Graphics graphics)
    {
        foreach (LightTrail trail in _trails)
        {
            DrawTrail(graphics, trail);
        }
    }

    /// <summary>
    /// Draws one glowing trail by painting a wide translucent glow and a thin core line.
    /// </summary>
    private static void DrawTrail(Graphics graphics, LightTrail trail)
    {
        if (trail.History.Count < 2)
        {
            return;
        }

        Color baseColor = GetTrailColor(trail.ColorFamily);

        for (int i = 1; i < trail.History.Count; i++)
        {
            float ageFactor = i / (float)(trail.History.Count - 1);
            float alphaFactor = MathF.Pow(ageFactor, 1.8f) * trail.Brightness;

            int glowAlpha = Math.Clamp((int)(54 * alphaFactor), 4, 80);
            int coreAlpha = Math.Clamp((int)(190 * alphaFactor), 12, 230);

            PointF from = trail.History[i - 1];
            PointF to = trail.History[i];

            using Pen glowPen = new(Color.FromArgb(glowAlpha, baseColor), trail.Thickness * 5.8f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            using Pen corePen = new(Color.FromArgb(coreAlpha, baseColor), trail.Thickness)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            graphics.DrawLine(glowPen, from, to);
            graphics.DrawLine(corePen, from, to);
        }
    }

    /// <summary>
    /// Draws bright heads for all active trails.
    /// </summary>
    private void DrawTrailHeads(Graphics graphics)
    {
        foreach (LightTrail trail in _trails)
        {
            Color baseColor = GetTrailColor(trail.ColorFamily);
            float radius = trail.Thickness * 2.4f;
            float glowRadius = radius * 3.8f;

            using SolidBrush glowBrush = new(Color.FromArgb(42, baseColor));
            using SolidBrush coreBrush = new(Color.FromArgb(230, 235, 255, 255));

            graphics.FillEllipse(
                glowBrush,
                trail.Position.X - glowRadius,
                trail.Position.Y - glowRadius,
                glowRadius * 2,
                glowRadius * 2);

            graphics.FillEllipse(
                coreBrush,
                trail.Position.X - radius,
                trail.Position.Y - radius,
                radius * 2,
                radius * 2);
        }
    }

    /// <summary>
    /// Returns the base color for a trail family.
    /// </summary>
    private static Color GetTrailColor(int colorFamily)
    {
        return colorFamily switch
        {
            0 => Color.FromArgb(95, 225, 255),
            1 => Color.FromArgb(105, 255, 205),
            2 => Color.FromArgb(175, 165, 255),
            _ => Color.FromArgb(255, 210, 130)
        };
    }

    /// <summary>
    /// Returns a random single-precision value in the requested interval.
    /// </summary>
    private float RandomRange(float minInclusive, float maxExclusive)
    {
        return minInclusive + (float)_random.NextDouble() * (maxExclusive - minInclusive);
    }

    /// <summary>
    /// Mutable state for one light trail.
    /// </summary>
    private sealed class LightTrail
    {
        public PointF Position { get; set; }
        public PointF Velocity { get; set; }
        public float BaseSpeed { get; set; }
        public float Phase { get; set; }
        public float WaveSpeed { get; set; }
        public float WaveStrength { get; set; }
        public float Thickness { get; set; }
        public float Brightness { get; set; }
        public int ColorFamily { get; set; }
        public List<PointF> History { get; } = [];
    }
}
