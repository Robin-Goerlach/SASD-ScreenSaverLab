using System.Drawing;
using System.Drawing.Drawing2D;
using Sasd.ScreenSaverLab.Core;

namespace Sasd.ScreenSaverLab.Effects;

/// <summary>
/// A calm star-field effect with slowly drifting particles and a subtle depth impression.
/// </summary>
/// <remarks>
/// This is intentionally not a physically correct star simulation. It is a small,
/// readable V0.1 effect that proves the host/effect architecture and already looks
/// pleasant enough to use as a prototype screensaver.
/// </remarks>
public sealed class StarDriftEffect : IScreenSaverEffect
{
    private const int DefaultStarCount = 320;
    private readonly Random _random = new();
    private readonly List<StarParticle> _stars = [];

    /// <inheritdoc />
    public string Name => "Star Drift";

    /// <inheritdoc />
    public string Description => "A calm SASD star-field with drifting particles and subtle glow.";

    /// <inheritdoc />
    public void Initialize(Size viewportSize)
    {
        _stars.Clear();

        int starCount = CalculateStarCount(viewportSize);

        for (int i = 0; i < starCount; i++)
        {
            _stars.Add(CreateRandomStar(viewportSize, randomizeVerticalPosition: true));
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

        // Clamp large time gaps, for example when the debugger paused the process.
        // This avoids a single giant animation step after continuing execution.
        seconds = MathF.Min(seconds, 0.1f);

        for (int i = 0; i < _stars.Count; i++)
        {
            StarParticle star = _stars[i];

            // Stars with a larger depth value appear closer to the viewer and move faster.
            float depthSpeed = 0.25f + star.Depth * 1.35f;

            star.X += star.HorizontalDrift * seconds * depthSpeed;
            star.Y += star.VerticalSpeed * seconds * depthSpeed;
            star.PulsePhase += seconds * star.PulseSpeed;

            // Wrap around the screen. Recreating the star on one side gives the field
            // a continuous feeling without having to allocate new particles every frame.
            if (star.Y > viewportSize.Height + 20 || star.X < -40 || star.X > viewportSize.Width + 40)
            {
                star = CreateRandomStar(viewportSize, randomizeVerticalPosition: false);
                star.Y = -20;
            }

            _stars[i] = star;
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
        graphics.CompositingQuality = CompositingQuality.HighSpeed;

        DrawBackground(graphics, viewportSize);
        DrawStars(graphics);
    }

    /// <summary>
    /// Calculates a particle count based on the screen size, but keeps it inside a useful range.
    /// </summary>
    private static int CalculateStarCount(Size viewportSize)
    {
        double megapixels = Math.Max(1.0, viewportSize.Width * viewportSize.Height / 1_000_000.0);
        int calculated = (int)(DefaultStarCount * megapixels * 0.85);

        return Math.Clamp(calculated, 180, 700);
    }

    /// <summary>
    /// Creates a star with random position, depth, brightness, and movement.
    /// </summary>
    private StarParticle CreateRandomStar(Size viewportSize, bool randomizeVerticalPosition)
    {
        float depth = RandomRange(0.15f, 1.0f);

        return new StarParticle
        {
            X = RandomRange(0, Math.Max(1, viewportSize.Width)),
            Y = randomizeVerticalPosition ? RandomRange(0, Math.Max(1, viewportSize.Height)) : -20,
            Depth = depth,
            Radius = RandomRange(0.8f, 2.6f) * (0.45f + depth),
            BaseAlpha = (int)RandomRange(70, 210),
            VerticalSpeed = RandomRange(8f, 34f),
            HorizontalDrift = RandomRange(-10f, 18f),
            PulsePhase = RandomRange(0f, MathF.PI * 2f),
            PulseSpeed = RandomRange(0.4f, 1.4f)
        };
    }

    /// <summary>
    /// Draws a dark blue-black background with a slight vertical gradient.
    /// </summary>
    private static void DrawBackground(Graphics graphics, Size viewportSize)
    {
        Rectangle bounds = new(0, 0, viewportSize.Width, viewportSize.Height);

        using LinearGradientBrush brush = new(
            bounds,
            Color.FromArgb(255, 2, 7, 16),
            Color.FromArgb(255, 6, 18, 36),
            LinearGradientMode.Vertical);

        graphics.FillRectangle(brush, bounds);
    }

    /// <summary>
    /// Draws all star particles with small glowing circles.
    /// </summary>
    private void DrawStars(Graphics graphics)
    {
        foreach (StarParticle star in _stars)
        {
            float pulse = 0.75f + MathF.Sin(star.PulsePhase) * 0.25f;
            int alpha = Math.Clamp((int)(star.BaseAlpha * pulse), 40, 230);

            float glowRadius = star.Radius * (2.4f + star.Depth);
            float coreRadius = star.Radius;

            using SolidBrush glowBrush = new(Color.FromArgb(alpha / 5, 90, 180, 255));
            using SolidBrush coreBrush = new(Color.FromArgb(alpha, 220, 242, 255));

            graphics.FillEllipse(
                glowBrush,
                star.X - glowRadius,
                star.Y - glowRadius,
                glowRadius * 2,
                glowRadius * 2);

            graphics.FillEllipse(
                coreBrush,
                star.X - coreRadius,
                star.Y - coreRadius,
                coreRadius * 2,
                coreRadius * 2);
        }
    }

    /// <summary>
    /// Returns a random single-precision value in the requested interval.
    /// </summary>
    private float RandomRange(float minInclusive, float maxExclusive)
    {
        return minInclusive + (float)_random.NextDouble() * (maxExclusive - minInclusive);
    }

    /// <summary>
    /// Internal mutable particle state.
    /// </summary>
    /// <remarks>
    /// A struct is sufficient here and avoids hundreds of tiny object allocations.
    /// The code is still intentionally simple and readable.
    /// </remarks>
    private struct StarParticle
    {
        public float X;
        public float Y;
        public float Depth;
        public float Radius;
        public int BaseAlpha;
        public float VerticalSpeed;
        public float HorizontalDrift;
        public float PulsePhase;
        public float PulseSpeed;
    }
}
