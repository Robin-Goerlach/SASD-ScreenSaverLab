using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Sasd.ScreenSaverLab.Core;

namespace Sasd.ScreenSaverLab.Effects;

/// <summary>
/// Draws an abstract system telemetry pulse visualization.
/// </summary>
/// <remarks>
/// The effect samples lightweight Windows/runtime telemetry where possible and turns it
/// into a calm abstract dashboard. CPU is sampled through the Windows GetSystemTimes API,
/// RAM through GlobalMemoryStatusEx and network throughput through .NET network-interface
/// counters. If a source is unavailable, the effect keeps running with safe fallback values.
/// </remarks>
public sealed class SystemPulseEffect : IScreenSaverEffect
{
    private const int HistoryLength = 96;
    private const int ParticleCount = 110;

    private readonly SystemTelemetrySampler _sampler = new();
    private readonly Queue<float> _cpuHistory = new();
    private readonly Queue<float> _ramHistory = new();
    private readonly Queue<float> _netHistory = new();
    private readonly List<PulseParticle> _particles = [];
    private readonly Random _random = new(1886);

    private float _time;
    private float _historyAccumulator;
    private float _cpu;
    private float _ram;
    private float _network;

    /// <inheritdoc />
    public string Name => "System Pulse";

    /// <inheritdoc />
    public string Description => "Abstract CPU, memory and network pulse visualization.";

    /// <inheritdoc />
    public void Initialize(Size viewportSize)
    {
        _time = 0f;
        _historyAccumulator = 0f;
        _cpu = 0f;
        _ram = 0f;
        _network = 0f;

        _sampler.Reset();
        _cpuHistory.Clear();
        _ramHistory.Clear();
        _netHistory.Clear();
        _particles.Clear();

        for (int index = 0; index < HistoryLength; index++)
        {
            _cpuHistory.Enqueue(0.12f);
            _ramHistory.Enqueue(0.35f);
            _netHistory.Enqueue(0.08f);
        }

        for (int index = 0; index < ParticleCount; index++)
        {
            _particles.Add(CreateParticle(index));
        }
    }

    /// <inheritdoc />
    public void Update(TimeSpan elapsed, Size viewportSize)
    {
        float seconds = (float)Math.Min(elapsed.TotalSeconds, 0.12);
        _time += seconds;
        _historyAccumulator += seconds;

        SystemTelemetrySnapshot snapshot = _sampler.Sample();

        // Smooth the metrics so brief measurement jumps look like a pulse instead of jitter.
        _cpu = Lerp(_cpu, snapshot.CpuLoad, 0.08f);
        _ram = Lerp(_ram, snapshot.MemoryLoad, 0.05f);
        _network = Lerp(_network, snapshot.NetworkLoad, 0.09f);

        if (_historyAccumulator >= 0.12f)
        {
            _historyAccumulator = 0f;
            EnqueueHistory(_cpuHistory, _cpu);
            EnqueueHistory(_ramHistory, _ram);
            EnqueueHistory(_netHistory, _network);
        }

        foreach (PulseParticle particle in _particles)
        {
            particle.Age += seconds * particle.Speed;
            if (particle.Age >= 1f)
            {
                ResetParticle(particle);
            }
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
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        RectangleF bounds = new(0, 0, viewportSize.Width, viewportSize.Height);
        DrawBackground(graphics, bounds);
        DrawPulseField(graphics, bounds);

        float margin = Math.Max(28f, Math.Min(viewportSize.Width, viewportSize.Height) * 0.035f);
        RectangleF content = RectangleF.Inflate(bounds, -margin, -margin);

        using Font titleFont = new("Segoe UI", Math.Clamp(viewportSize.Width / 64f, 18f, 34f), FontStyle.Regular, GraphicsUnit.Point);
        using Font labelFont = new("Consolas", Math.Clamp(viewportSize.Width / 140f, 9f, 15f), FontStyle.Regular, GraphicsUnit.Point);
        using Font valueFont = new("Consolas", Math.Clamp(viewportSize.Width / 96f, 12f, 22f), FontStyle.Bold, GraphicsUnit.Point);
        using Font smallFont = new("Consolas", Math.Clamp(viewportSize.Width / 175f, 8f, 11f), FontStyle.Regular, GraphicsUnit.Point);

        DrawHeader(graphics, content, titleFont, smallFont);
        DrawCentralPulse(graphics, content);
        DrawMetricCards(graphics, content, labelFont, valueFont, smallFont);
        DrawFooter(graphics, content, smallFont);
    }

    /// <summary>
    /// Creates one particle for the pulse field.
    /// </summary>
    private PulseParticle CreateParticle(int index)
    {
        PulseParticle particle = new();
        ResetParticle(particle);
        particle.Age = (float)_random.NextDouble();
        particle.Family = index % 3;
        return particle;
    }

    /// <summary>
    /// Resets one particle to a new outward pulse path.
    /// </summary>
    private void ResetParticle(PulseParticle particle)
    {
        particle.Angle = RandomRange(0f, MathF.PI * 2f);
        particle.Speed = RandomRange(0.12f, 0.46f);
        particle.Distance = RandomRange(0.20f, 1.0f);
        particle.Size = RandomRange(1.2f, 4.5f);
        particle.Phase = RandomRange(0f, MathF.PI * 2f);
        particle.Age = 0f;
    }

    /// <summary>
    /// Draws the dark dashboard background.
    /// </summary>
    private static void DrawBackground(Graphics graphics, RectangleF bounds)
    {
        using LinearGradientBrush gradient = new(
            Rectangle.Round(bounds),
            Color.FromArgb(255, 2, 4, 10),
            Color.FromArgb(255, 2, 18, 32),
            LinearGradientMode.ForwardDiagonal);
        graphics.FillRectangle(gradient, bounds);

        using SolidBrush veil = new(Color.FromArgb(48, 0, 0, 0));
        graphics.FillRectangle(veil, bounds);
    }

    /// <summary>
    /// Draws abstract moving particles around the center.
    /// </summary>
    private void DrawPulseField(Graphics graphics, RectangleF bounds)
    {
        PointF center = new(bounds.Left + bounds.Width * 0.5f, bounds.Top + bounds.Height * 0.52f);
        float maxRadius = Math.Min(bounds.Width, bounds.Height) * 0.48f;
        float intensity = Math.Clamp((_cpu * 0.55f) + (_network * 0.35f) + 0.10f, 0.08f, 1f);

        foreach (PulseParticle particle in _particles)
        {
            float age = Math.Clamp(particle.Age, 0f, 1f);
            float radius = maxRadius * particle.Distance * age;
            float wobble = MathF.Sin(_time * 1.7f + particle.Phase) * 0.065f;
            float angle = particle.Angle + wobble;
            float x = center.X + MathF.Cos(angle) * radius;
            float y = center.Y + MathF.Sin(angle) * radius * 0.72f;
            float alpha = (1f - age) * (0.24f + intensity * 0.76f);
            Color color = particle.Family switch
            {
                0 => Color.FromArgb((int)(150 * alpha), 92, 240, 255),
                1 => Color.FromArgb((int)(145 * alpha), 255, 210, 110),
                _ => Color.FromArgb((int)(130 * alpha), 170, 255, 190)
            };

            using SolidBrush brush = new(color);
            float size = particle.Size * (0.8f + intensity * 1.25f);
            graphics.FillEllipse(brush, x - size * 0.5f, y - size * 0.5f, size, size);
        }
    }

    /// <summary>
    /// Draws the header title and mode line.
    /// </summary>
    private void DrawHeader(Graphics graphics, RectangleF content, Font titleFont, Font smallFont)
    {
        using SolidBrush titleBrush = new(Color.FromArgb(225, 225, 245, 255));
        using SolidBrush smallBrush = new(Color.FromArgb(150, 175, 215, 235));
        using Pen line = new(Color.FromArgb(80, 90, 220, 255), 1f);

        RectangleF titleBounds = new(content.Left, content.Top, content.Width, titleFont.GetHeight(graphics) + 6f);
        DrawText(graphics, "SASD SYSTEM PULSE", titleFont, titleBrush, titleBounds, StringAlignment.Near, StringAlignment.Near);

        RectangleF modeBounds = new(content.Left, titleBounds.Bottom + 4f, content.Width, smallFont.GetHeight(graphics) + 5f);
        string mode = $"CPU/RAM/NET telemetry visualization  //  local session  //  {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        DrawText(graphics, mode, smallFont, smallBrush, modeBounds, StringAlignment.Near, StringAlignment.Near);
        graphics.DrawLine(line, content.Left, modeBounds.Bottom + 8f, content.Right, modeBounds.Bottom + 8f);
    }

    /// <summary>
    /// Draws the central radar-like pulse rings.
    /// </summary>
    private void DrawCentralPulse(Graphics graphics, RectangleF content)
    {
        PointF center = new(content.Left + content.Width * 0.50f, content.Top + content.Height * 0.53f);
        float baseRadius = Math.Min(content.Width, content.Height) * 0.11f;
        float intensity = Math.Clamp((_cpu * 0.50f) + (_ram * 0.20f) + (_network * 0.40f), 0f, 1f);

        for (int index = 0; index < 9; index++)
        {
            float pulse = ((_time * (0.18f + intensity * 0.20f)) + index * 0.12f) % 1f;
            float radius = baseRadius * (0.9f + index * 0.62f + pulse * 0.34f);
            int alpha = (int)Math.Clamp(118 - index * 9 + intensity * 45f - pulse * 55f, 18f, 165f);
            using Pen ringPen = new(Color.FromArgb(alpha, 90, 230, 255), 1.2f);
            graphics.DrawEllipse(ringPen, center.X - radius, center.Y - radius * 0.72f, radius * 2f, radius * 1.44f);
        }

        using GraphicsPath corePath = new();
        float coreRadius = baseRadius * (0.62f + intensity * 0.42f + MathF.Sin(_time * 2.8f) * 0.04f);
        corePath.AddEllipse(center.X - coreRadius, center.Y - coreRadius, coreRadius * 2f, coreRadius * 2f);
        using PathGradientBrush glow = new(corePath)
        {
            CenterColor = Color.FromArgb(135, 255, 226, 125),
            SurroundColors = [Color.FromArgb(0, 70, 220, 255)]
        };
        graphics.FillPath(glow, corePath);

        using Pen cross = new(Color.FromArgb(110, 140, 240, 255), 1f);
        graphics.DrawLine(cross, center.X - baseRadius * 3.5f, center.Y, center.X + baseRadius * 3.5f, center.Y);
        graphics.DrawLine(cross, center.X, center.Y - baseRadius * 2.4f, center.X, center.Y + baseRadius * 2.4f);
    }

    /// <summary>
    /// Draws three metric cards for CPU, memory and network activity.
    /// </summary>
    private void DrawMetricCards(Graphics graphics, RectangleF content, Font labelFont, Font valueFont, Font smallFont)
    {
        float cardGap = Math.Max(14f, content.Width * 0.018f);
        float cardHeight = Math.Max(145f, content.Height * 0.22f);
        float cardWidth = (content.Width - cardGap * 2f) / 3f;
        float y = content.Bottom - cardHeight - Math.Max(42f, content.Height * 0.07f);

        DrawMetricCard(
            graphics,
            new RectangleF(content.Left, y, cardWidth, cardHeight),
            "CPU",
            _cpu,
            _cpuHistory.ToArray(),
            "SYSTEM LOAD",
            Color.FromArgb(125, 235, 255),
            labelFont,
            valueFont,
            smallFont);

        DrawMetricCard(
            graphics,
            new RectangleF(content.Left + cardWidth + cardGap, y, cardWidth, cardHeight),
            "RAM",
            _ram,
            _ramHistory.ToArray(),
            "MEMORY LOAD",
            Color.FromArgb(255, 214, 115),
            labelFont,
            valueFont,
            smallFont);

        DrawMetricCard(
            graphics,
            new RectangleF(content.Left + (cardWidth + cardGap) * 2f, y, cardWidth, cardHeight),
            "NET",
            _network,
            _netHistory.ToArray(),
            "NETWORK ACTIVITY",
            Color.FromArgb(150, 255, 190),
            labelFont,
            valueFont,
            smallFont);
    }

    /// <summary>
    /// Draws one metric card with a value, bar and sparkline.
    /// </summary>
    private static void DrawMetricCard(
        Graphics graphics,
        RectangleF bounds,
        string headline,
        float value,
        float[] history,
        string description,
        Color color,
        Font labelFont,
        Font valueFont,
        Font smallFont)
    {
        using SolidBrush panelBrush = new(Color.FromArgb(42, 4, 18, 28));
        using Pen glow = new(Color.FromArgb(38, color.R, color.G, color.B), 6f);
        using Pen border = new(Color.FromArgb(135, color.R, color.G, color.B), 1.3f);
        graphics.FillRectangle(panelBrush, bounds);
        graphics.DrawRectangle(glow, Rectangle.Round(bounds));
        graphics.DrawRectangle(border, Rectangle.Round(bounds));

        using SolidBrush labelBrush = new(Color.FromArgb(215, color.R, color.G, color.B));
        using SolidBrush valueBrush = new(Color.FromArgb(235, 232, 246, 255));
        using SolidBrush dimBrush = new(Color.FromArgb(150, 175, 205, 218));

        RectangleF labelBounds = new(bounds.Left + 16f, bounds.Top + 12f, bounds.Width - 32f, labelFont.GetHeight(graphics) + 4f);
        DrawText(graphics, description, labelFont, labelBrush, labelBounds, StringAlignment.Near, StringAlignment.Near);

        RectangleF valueBounds = new(bounds.Left + 16f, labelBounds.Bottom + 4f, bounds.Width - 32f, valueFont.GetHeight(graphics) + 6f);
        DrawText(graphics, $"{headline} {value * 100f:0}%", valueFont, valueBrush, valueBounds, StringAlignment.Near, StringAlignment.Near);

        float barLeft = bounds.Left + 16f;
        float barRight = bounds.Right - 16f;
        float barY = valueBounds.Bottom + 16f;
        using Pen backBar = new(Color.FromArgb(45, color.R, color.G, color.B), 7f);
        using Pen activeBar = new(Color.FromArgb(205, color.R, color.G, color.B), 7f);
        graphics.DrawLine(backBar, barLeft, barY, barRight, barY);
        graphics.DrawLine(activeBar, barLeft, barY, barLeft + (barRight - barLeft) * Math.Clamp(value, 0f, 1f), barY);

        RectangleF sparkBounds = new(bounds.Left + 16f, barY + 18f, bounds.Width - 32f, Math.Max(28f, bounds.Bottom - barY - 42f));
        DrawSparkline(graphics, history, sparkBounds, color);

        RectangleF noteBounds = new(bounds.Left + 16f, bounds.Bottom - smallFont.GetHeight(graphics) - 10f, bounds.Width - 32f, smallFont.GetHeight(graphics) + 4f);
        DrawText(graphics, "sampled locally, visualized abstractly", smallFont, dimBrush, noteBounds, StringAlignment.Near, StringAlignment.Near);
    }

    /// <summary>
    /// Draws a short technical note at the bottom.
    /// </summary>
    private static void DrawFooter(Graphics graphics, RectangleF content, Font smallFont)
    {
        using SolidBrush brush = new(Color.FromArgb(120, 165, 205, 225));
        RectangleF footer = new(content.Left, content.Bottom - smallFont.GetHeight(graphics) - 2f, content.Width, smallFont.GetHeight(graphics) + 3f);
        DrawText(graphics, "ESC / mouse click / mouse move exits  //  System Pulse is a screensaver visualization, not a monitoring replacement", smallFont, brush, footer, StringAlignment.Center, StringAlignment.Center);
    }

    /// <summary>
    /// Draws one metric history curve.
    /// </summary>
    private static void DrawSparkline(Graphics graphics, float[] values, RectangleF bounds, Color color)
    {
        if (values.Length < 2 || bounds.Width <= 1f || bounds.Height <= 1f)
        {
            return;
        }

        using Pen gridPen = new(Color.FromArgb(30, color.R, color.G, color.B), 1f);
        for (int i = 0; i <= 3; i++)
        {
            float y = bounds.Top + bounds.Height * (i / 3f);
            graphics.DrawLine(gridPen, bounds.Left, y, bounds.Right, y);
        }

        PointF[] points = new PointF[values.Length];
        for (int index = 0; index < values.Length; index++)
        {
            float x = bounds.Left + bounds.Width * index / (values.Length - 1);
            float y = bounds.Bottom - bounds.Height * Math.Clamp(values[index], 0f, 1f);
            points[index] = new PointF(x, y);
        }

        using Pen glow = new(Color.FromArgb(65, color.R, color.G, color.B), 5f);
        using Pen line = new(Color.FromArgb(220, color.R, color.G, color.B), 1.7f);
        graphics.DrawLines(glow, points);
        graphics.DrawLines(line, points);
    }

    /// <summary>
    /// Enqueues one value and keeps metric histories bounded.
    /// </summary>
    private static void EnqueueHistory(Queue<float> history, float value)
    {
        history.Enqueue(Math.Clamp(value, 0f, 1f));
        while (history.Count > HistoryLength)
        {
            history.Dequeue();
        }
    }

    /// <summary>
    /// Draws clipped text with ellipsis.
    /// </summary>
    private static void DrawText(Graphics graphics, string text, Font font, Brush brush, RectangleF bounds, StringAlignment horizontal, StringAlignment vertical)
    {
        using StringFormat format = new()
        {
            Alignment = horizontal,
            LineAlignment = vertical,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap
        };

        graphics.DrawString(text, font, brush, bounds, format);
    }

    private static float Lerp(float current, float target, float amount)
    {
        return current + (target - current) * Math.Clamp(amount, 0f, 1f);
    }

    private float RandomRange(float min, float max)
    {
        return min + (float)_random.NextDouble() * (max - min);
    }

    /// <summary>
    /// Particle state for the abstract pulse field.
    /// </summary>
    private sealed class PulseParticle
    {
        public float Angle { get; set; }

        public float Speed { get; set; }

        public float Distance { get; set; }

        public float Size { get; set; }

        public float Phase { get; set; }

        public float Age { get; set; }

        public int Family { get; set; }
    }

    /// <summary>
    /// Lightweight telemetry snapshot normalized to values between 0 and 1.
    /// </summary>
    private readonly record struct SystemTelemetrySnapshot(float CpuLoad, float MemoryLoad, float NetworkLoad);

    /// <summary>
    /// Samples lightweight telemetry for the abstract System Pulse effect.
    /// </summary>
    private sealed class SystemTelemetrySampler
    {
        private ulong _lastIdleTime;
        private ulong _lastKernelTime;
        private ulong _lastUserTime;
        private long _lastNetworkBytes;
        private DateTime _lastNetworkSampleUtc;
        private bool _hasCpuSample;
        private bool _hasNetworkSample;

        public void Reset()
        {
            _lastIdleTime = 0;
            _lastKernelTime = 0;
            _lastUserTime = 0;
            _lastNetworkBytes = 0;
            _lastNetworkSampleUtc = DateTime.UtcNow;
            _hasCpuSample = false;
            _hasNetworkSample = false;
        }

        public SystemTelemetrySnapshot Sample()
        {
            float cpu = SampleCpuLoad();
            float memory = SampleMemoryLoad();
            float network = SampleNetworkLoad();
            return new SystemTelemetrySnapshot(cpu, memory, network);
        }

        private float SampleCpuLoad()
        {
            if (!GetSystemTimes(out FileTime idleTime, out FileTime kernelTime, out FileTime userTime))
            {
                return 0.10f;
            }

            ulong idle = idleTime.ToUInt64();
            ulong kernel = kernelTime.ToUInt64();
            ulong user = userTime.ToUInt64();

            if (!_hasCpuSample)
            {
                _lastIdleTime = idle;
                _lastKernelTime = kernel;
                _lastUserTime = user;
                _hasCpuSample = true;
                return 0.10f;
            }

            ulong idleDelta = idle - _lastIdleTime;
            ulong kernelDelta = kernel - _lastKernelTime;
            ulong userDelta = user - _lastUserTime;
            ulong totalDelta = kernelDelta + userDelta;

            _lastIdleTime = idle;
            _lastKernelTime = kernel;
            _lastUserTime = user;

            if (totalDelta == 0)
            {
                return 0.10f;
            }

            double busy = 1.0 - (idleDelta / (double)totalDelta);
            return (float)Math.Clamp(busy, 0.0, 1.0);
        }

        private static float SampleMemoryLoad()
        {
            MemoryStatusEx status = new();
            status.Initialize();

            if (!GlobalMemoryStatusEx(ref status))
            {
                return 0.35f;
            }

            return Math.Clamp(status.MemoryLoad / 100f, 0f, 1f);
        }

        private float SampleNetworkLoad()
        {
            DateTime now = DateTime.UtcNow;
            long bytes = GetNetworkByteCounter();

            if (!_hasNetworkSample)
            {
                _lastNetworkBytes = bytes;
                _lastNetworkSampleUtc = now;
                _hasNetworkSample = true;
                return 0.05f;
            }

            double elapsedSeconds = Math.Max(0.05, (now - _lastNetworkSampleUtc).TotalSeconds);
            long byteDelta = Math.Max(0, bytes - _lastNetworkBytes);
            double bytesPerSecond = byteDelta / elapsedSeconds;

            _lastNetworkBytes = bytes;
            _lastNetworkSampleUtc = now;

            // Logarithmic scaling keeps both tiny background traffic and larger transfers visible.
            double normalized = Math.Log10(1.0 + bytesPerSecond) / 7.0;
            return (float)Math.Clamp(normalized, 0.0, 1.0);
        }

        private static long GetNetworkByteCounter()
        {
            long total = 0;

            try
            {
                foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                        networkInterface.OperationalStatus != OperationalStatus.Up)
                    {
                        continue;
                    }

                    IPv4InterfaceStatistics stats = networkInterface.GetIPv4Statistics();
                    total += Math.Max(0, stats.BytesReceived);
                    total += Math.Max(0, stats.BytesSent);
                }
            }
            catch (NetworkInformationException)
            {
                return 0;
            }
            catch (PlatformNotSupportedException)
            {
                return 0;
            }

            return total;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemTimes(out FileTime idleTime, out FileTime kernelTime, out FileTime userTime);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);

        [StructLayout(LayoutKind.Sequential)]
        private struct FileTime
        {
            public uint LowDateTime;

            public uint HighDateTime;

            public readonly ulong ToUInt64()
            {
                return ((ulong)HighDateTime << 32) | LowDateTime;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MemoryStatusEx
        {
            public uint Length;

            public uint MemoryLoad;

            public ulong TotalPhysical;

            public ulong AvailablePhysical;

            public ulong TotalPageFile;

            public ulong AvailablePageFile;

            public ulong TotalVirtual;

            public ulong AvailableVirtual;

            public ulong AvailableExtendedVirtual;

            public void Initialize()
            {
                Length = (uint)Marshal.SizeOf<MemoryStatusEx>();
            }
        }
    }
}
