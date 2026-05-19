using System.Drawing;
using System.Drawing.Drawing2D;
using Sasd.ScreenSaverLab.Core;

namespace Sasd.ScreenSaverLab.Effects;

/// <summary>
/// Draws a calm scientific-looking orbit field made from particles, ellipses and attractors.
/// </summary>
/// <remarks>
/// The effect aims at a research-lab or simulation aesthetic: particles move on slightly
/// tilted orbital paths around a few slow attractors. It is intentionally abstract and
/// original, but it gives SASD ScreenSaver Lab a more scientific visual personality.
/// </remarks>
public sealed class OrbitFieldEffect : IScreenSaverEffect
{
    private const int MinimumParticleCount = 90;
    private const int MaximumParticleCount = 420;
    private const int OrbitGuideCount = 9;

    private readonly Random _random = new();
    private readonly List<OrbitParticle> _particles = [];
    private float _time;

    /// <inheritdoc />
    public string Name => "Orbit Field";

    /// <inheritdoc />
    public string Description => "Scientific particle orbits with soft attractor field lines.";

    /// <inheritdoc />
    public void Initialize(Size viewportSize)
    {
        _time = 0f;
        _particles.Clear();

        int particleCount = CalculateParticleCount(viewportSize);
        for (int index = 0; index < particleCount; index++)
        {
            _particles.Add(CreateParticle(index));
        }
    }

    /// <inheritdoc />
    public void Update(TimeSpan elapsed, Size viewportSize)
    {
        float seconds = (float)Math.Min(elapsed.TotalSeconds, 0.12);
        _time += seconds;

        // If the window was moved to a screen with a very different size, gently adjust
        // density on the next frames without rebuilding constantly.
        int desiredCount = CalculateParticleCount(viewportSize);
        while (_particles.Count < desiredCount)
        {
            _particles.Add(CreateParticle(_particles.Count));
        }

        while (_particles.Count > desiredCount && _particles.Count > MinimumParticleCount)
        {
            _particles.RemoveAt(_particles.Count - 1);
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
        graphics.CompositingQuality = CompositingQuality.HighQuality;

        DrawBackground(graphics, viewportSize);
        DrawOrbitGuides(graphics, viewportSize);
        DrawFieldLines(graphics, viewportSize);
        DrawParticles(graphics, viewportSize);
        DrawAttractors(graphics, viewportSize);
    }

    /// <summary>
    /// Calculates particle density based on screen area.
    /// </summary>
    private static int CalculateParticleCount(Size viewportSize)
    {
        double megapixels = Math.Max(0.6, viewportSize.Width * viewportSize.Height / 1_000_000.0);
        int calculated = (int)(120 * megapixels);
        return Math.Clamp(calculated, MinimumParticleCount, MaximumParticleCount);
    }

    /// <summary>
    /// Creates randomized orbital state for one particle.
    /// </summary>
    private OrbitParticle CreateParticle(int index)
    {
        return new OrbitParticle
        {
            Radius = RandomRange(0.10f, 1.0f),
            Angle = RandomRange(0f, (MathF.PI * 2f)),
            Speed = RandomRange(0.10f, 0.55f) * (index % 2 == 0 ? 1f : -1f),
            Tilt = RandomRange(-0.72f, 0.72f),
            Phase = RandomRange(0f, (MathF.PI * 2f)),
            Size = RandomRange(1.2f, 3.6f),
            Brightness = RandomRange(0.45f, 1.0f),
            ColorFamily = index % 4
        };
    }

    /// <summary>
    /// Draws the quiet deep-space / lab-monitor background.
    /// </summary>
    private static void DrawBackground(Graphics graphics, Size viewportSize)
    {
        Rectangle bounds = new(0, 0, viewportSize.Width, viewportSize.Height);
        using LinearGradientBrush brush = new(
            bounds,
            Color.FromArgb(255, 2, 4, 10),
            Color.FromArgb(255, 2, 18, 28),
            LinearGradientMode.ForwardDiagonal);
        graphics.FillRectangle(brush, bounds);

        using SolidBrush veil = new(Color.FromArgb(34, 0, 0, 0));
        graphics.FillRectangle(veil, bounds);
    }

    /// <summary>
    /// Draws slow elliptical guide orbits around the center.
    /// </summary>
    private void DrawOrbitGuides(Graphics graphics, Size viewportSize)
    {
        PointF center = GetCenter(viewportSize);
        float baseRadius = Math.Min(viewportSize.Width, viewportSize.Height) * 0.08f;

        for (int index = 0; index < OrbitGuideCount; index++)
        {
            float radius = baseRadius * (1.15f + index * 0.55f);
            float width = radius * (2.35f + MathF.Sin(_time * 0.2f + index) * 0.08f);
            float height = radius * (0.78f + MathF.Cos(_time * 0.18f + index) * 0.05f);
            float alpha = Math.Clamp(62 - index * 4, 22, 70);

            using Matrix originalTransform = graphics.Transform.Clone();
            try
            {
                graphics.TranslateTransform(center.X, center.Y);
                graphics.RotateTransform(index * 17f + MathF.Sin(_time * 0.1f) * 4f);

                using Pen guidePen = new(Color.FromArgb((int)alpha, 120, 230, 255), 1.1f);
                graphics.DrawEllipse(guidePen, -width * 0.5f, -height * 0.5f, width, height);
            }
            finally
            {
                graphics.Transform = originalTransform;
            }
        }
    }

    /// <summary>
    /// Draws a few soft field lines between moving attractor points.
    /// </summary>
    private void DrawFieldLines(Graphics graphics, Size viewportSize)
    {
        PointF[] attractors = GetAttractors(viewportSize);
        using Pen pen = new(Color.FromArgb(42, 130, 245, 255), 1f);

        for (int i = 0; i < attractors.Length; i++)
        {
            for (int j = i + 1; j < attractors.Length; j++)
            {
                DrawCurvedFieldLine(graphics, pen, attractors[i], attractors[j], i + j);
            }
        }
    }

    /// <summary>
    /// Draws one curved field connection.
    /// </summary>
    private void DrawCurvedFieldLine(Graphics graphics, Pen pen, PointF a, PointF b, int seed)
    {
        PointF mid = new((a.X + b.X) * 0.5f, (a.Y + b.Y) * 0.5f);
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;
        float length = MathF.Sqrt(dx * dx + dy * dy);

        if (length < 1f)
        {
            return;
        }

        PointF normal = new(-dy / length, dx / length);
        float bend = MathF.Sin(_time * 0.35f + seed) * length * 0.12f;
        PointF control = new(mid.X + normal.X * bend, mid.Y + normal.Y * bend);

        using GraphicsPath path = new();
        path.AddBezier(a, control, control, b);
        graphics.DrawPath(pen, path);
    }

    /// <summary>
    /// Draws all particles at their current orbital positions.
    /// </summary>
    private void DrawParticles(Graphics graphics, Size viewportSize)
    {
        PointF center = GetCenter(viewportSize);
        float maxRadius = Math.Min(viewportSize.Width, viewportSize.Height) * 0.47f;

        foreach (OrbitParticle particle in _particles)
        {
            PointF position = CalculateParticlePosition(particle, center, maxRadius);
            float glowSize = particle.Size * (2.4f + particle.Brightness * 1.6f);
            Color color = GetParticleColor(particle);

            using SolidBrush glow = new(Color.FromArgb((int)(28 * particle.Brightness), color.R, color.G, color.B));
            graphics.FillEllipse(glow, position.X - glowSize, position.Y - glowSize, glowSize * 2f, glowSize * 2f);

            using SolidBrush core = new(Color.FromArgb((int)(165 * particle.Brightness), color.R, color.G, color.B));
            graphics.FillEllipse(core, position.X - particle.Size * 0.5f, position.Y - particle.Size * 0.5f, particle.Size, particle.Size);
        }
    }

    /// <summary>
    /// Draws the moving attractor points after the small particle layer.
    /// </summary>
    private void DrawAttractors(Graphics graphics, Size viewportSize)
    {
        PointF[] attractors = GetAttractors(viewportSize);

        foreach (PointF attractor in attractors)
        {
            using GraphicsPath path = new();
            path.AddEllipse(attractor.X - 26f, attractor.Y - 26f, 52f, 52f);

            using PathGradientBrush glow = new(path)
            {
                CenterColor = Color.FromArgb(100, 255, 245, 190),
                SurroundColors = [Color.FromArgb(0, 255, 180, 80)]
            };
            graphics.FillPath(glow, path);

            using Pen ring = new(Color.FromArgb(135, 255, 225, 150), 1.2f);
            graphics.DrawEllipse(ring, attractor.X - 11f, attractor.Y - 11f, 22f, 22f);
        }
    }

    /// <summary>
    /// Calculates one particle position on a tilted, slightly breathing ellipse.
    /// </summary>
    private PointF CalculateParticlePosition(OrbitParticle particle, PointF center, float maxRadius)
    {
        float angle = particle.Angle + _time * particle.Speed;
        float breathing = 1f + MathF.Sin(_time * 0.55f + particle.Phase) * 0.065f;
        float radius = maxRadius * particle.Radius * breathing;
        float x = MathF.Cos(angle) * radius;
        float y = MathF.Sin(angle) * radius * (0.42f + MathF.Abs(particle.Tilt) * 0.34f);

        float rotation = particle.Tilt + MathF.Sin(_time * 0.18f) * 0.15f;
        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);

        return new PointF(
            center.X + x * cos - y * sin,
            center.Y + x * sin + y * cos);
    }

    /// <summary>
    /// Returns the center point, slightly animated so the field feels alive.
    /// </summary>
    private PointF GetCenter(Size viewportSize)
    {
        return new PointF(
            viewportSize.Width * 0.5f + MathF.Sin(_time * 0.13f) * viewportSize.Width * 0.025f,
            viewportSize.Height * 0.52f + MathF.Cos(_time * 0.11f) * viewportSize.Height * 0.025f);
    }

    /// <summary>
    /// Calculates the current attractor points.
    /// </summary>
    private PointF[] GetAttractors(Size viewportSize)
    {
        PointF center = GetCenter(viewportSize);
        float radius = Math.Min(viewportSize.Width, viewportSize.Height) * 0.31f;

        return
        [
            new PointF(center.X + MathF.Cos(_time * 0.34f) * radius, center.Y + MathF.Sin(_time * 0.29f) * radius * 0.55f),
            new PointF(center.X + MathF.Cos(_time * -0.27f + 2.2f) * radius * 0.86f, center.Y + MathF.Sin(_time * -0.31f + 2.2f) * radius * 0.66f),
            new PointF(center.X + MathF.Cos(_time * 0.21f + 4.1f) * radius * 0.72f, center.Y + MathF.Sin(_time * 0.37f + 4.1f) * radius * 0.74f)
        ];
    }

    /// <summary>
    /// Selects a restrained particle color by family.
    /// </summary>
    private static Color GetParticleColor(OrbitParticle particle)
    {
        return particle.ColorFamily switch
        {
            0 => Color.FromArgb(135, 235, 255),
            1 => Color.FromArgb(255, 215, 150),
            2 => Color.FromArgb(220, 160, 255),
            _ => Color.FromArgb(150, 255, 210)
        };
    }

    private float RandomRange(float minimum, float maximum)
    {
        return minimum + (float)_random.NextDouble() * (maximum - minimum);
    }

    private sealed class OrbitParticle
    {
        public float Radius { get; init; }
        public float Angle { get; init; }
        public float Speed { get; init; }
        public float Tilt { get; init; }
        public float Phase { get; init; }
        public float Size { get; init; }
        public float Brightness { get; init; }
        public int ColorFamily { get; init; }
    }
}
