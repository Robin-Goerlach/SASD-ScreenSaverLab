using System.Drawing;
using System.Drawing.Drawing2D;
using Sasd.ScreenSaverLab.Core;

namespace Sasd.ScreenSaverLab.Effects;

/// <summary>
/// Draws a calm fictional laboratory and research-data console.
/// </summary>
/// <remarks>
/// The effect is intentionally not connected to real laboratory systems. It creates a
/// believable research-console atmosphere using generated sample rows, signal values,
/// status messages and small sparklines. A later version may replace the generated data
/// with configured local files or live project data, but V0.6.0 keeps the effect safe,
/// deterministic and independent from external services.
/// </remarks>
public sealed class LabConsoleEffect : IScreenSaverEffect
{
    private const int SampleCount = 18;
    private const int SignalPointCount = 96;

    private readonly Random _random = new(7603);
    private readonly List<LabSample> _samples = [];
    private readonly Queue<float> _primarySignal = new();
    private readonly Queue<float> _secondarySignal = new();
    private readonly string[] _eventMessages =
    [
        "normalizing fluorescence curve",
        "validating sample metadata",
        "baseline drift within expected range",
        "synchronizing instrument timestamp",
        "checking replicate consistency",
        "export queue idle",
        "model confidence above threshold",
        "sensor noise compensation active",
        "reference channel stable",
        "batch integrity marker verified"
    ];

    private float _time;
    private float _eventScroll;

    /// <inheritdoc />
    public string Name => "Lab Console";

    /// <inheritdoc />
    public string Description => "Fictional research-data console with lab samples, signals and status output.";

    /// <inheritdoc />
    public void Initialize(Size viewportSize)
    {
        _time = 0f;
        _eventScroll = 0f;
        _samples.Clear();
        _primarySignal.Clear();
        _secondarySignal.Clear();

        for (int index = 0; index < SampleCount; index++)
        {
            _samples.Add(CreateSample(index));
        }

        for (int index = 0; index < SignalPointCount; index++)
        {
            float t = index / (float)(SignalPointCount - 1);
            _primarySignal.Enqueue(CreateSignalValue(t, phase: 0f));
            _secondarySignal.Enqueue(CreateSignalValue(t, phase: 1.8f));
        }
    }

    /// <inheritdoc />
    public void Update(TimeSpan elapsed, Size viewportSize)
    {
        float seconds = (float)Math.Min(elapsed.TotalSeconds, 0.12);
        _time += seconds;
        _eventScroll += seconds * 0.42f;

        for (int index = 0; index < _samples.Count; index++)
        {
            LabSample sample = _samples[index];
            sample.Signal = Math.Clamp(
                sample.BaseSignal + MathF.Sin(_time * sample.Speed + sample.Phase) * 7.5f + MathF.Sin(_time * 0.21f + index) * 2.2f,
                0f,
                100f);
            sample.Temperature = Math.Clamp(
                sample.BaseTemperature + MathF.Sin(_time * 0.37f + sample.Phase) * 0.34f,
                18f,
                28f);
            sample.Ph = Math.Clamp(
                sample.BasePh + MathF.Sin(_time * 0.29f + sample.Phase * 1.7f) * 0.06f,
                5.8f,
                8.6f);
        }

        EnqueueSignal(_primarySignal, CreateSignalValue(_time * 0.05f, phase: 0f));
        EnqueueSignal(_secondarySignal, CreateSignalValue(_time * 0.05f, phase: 1.8f));
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
        DrawScanlines(graphics, bounds);

        float margin = Math.Max(24f, Math.Min(viewportSize.Width, viewportSize.Height) * 0.032f);
        RectangleF consoleBounds = RectangleF.Inflate(bounds, -margin, -margin);
        DrawOuterFrame(graphics, consoleBounds);

        using Font titleFont = new("Consolas", Math.Clamp(viewportSize.Width / 72f, 16f, 28f), FontStyle.Bold, GraphicsUnit.Point);
        using Font monoFont = new("Consolas", Math.Clamp(viewportSize.Width / 146f, 9f, 14f), FontStyle.Regular, GraphicsUnit.Point);
        using Font smallFont = new("Consolas", Math.Clamp(viewportSize.Width / 170f, 8f, 11f), FontStyle.Regular, GraphicsUnit.Point);
        using Font tinyFont = new("Consolas", Math.Clamp(viewportSize.Width / 200f, 7f, 10f), FontStyle.Regular, GraphicsUnit.Point);

        float headerHeight = titleFont.GetHeight(graphics) + 34f;
        RectangleF headerBounds = new(consoleBounds.Left + 22f, consoleBounds.Top + 18f, consoleBounds.Width - 44f, headerHeight);
        DrawHeader(graphics, headerBounds, titleFont, smallFont);

        float footerHeight = Math.Max(52f, smallFont.GetHeight(graphics) * 3.2f);
        RectangleF contentBounds = new(
            consoleBounds.Left + 22f,
            headerBounds.Bottom + 16f,
            consoleBounds.Width - 44f,
            consoleBounds.Height - headerHeight - footerHeight - 54f);

        float leftWidth = contentBounds.Width * 0.58f;
        RectangleF tableBounds = new(contentBounds.Left, contentBounds.Top, leftWidth - 12f, contentBounds.Height);
        RectangleF rightBounds = new(contentBounds.Left + leftWidth + 12f, contentBounds.Top, contentBounds.Width - leftWidth - 12f, contentBounds.Height);

        DrawSampleTable(graphics, tableBounds, monoFont, smallFont, tinyFont);
        DrawSignalPanels(graphics, rightBounds, monoFont, smallFont, tinyFont);

        RectangleF footerBounds = new(consoleBounds.Left + 22f, consoleBounds.Bottom - footerHeight - 18f, consoleBounds.Width - 44f, footerHeight);
        DrawEventLog(graphics, footerBounds, smallFont, tinyFont);
    }

    /// <summary>
    /// Creates one generated lab sample row.
    /// </summary>
    private LabSample CreateSample(int index)
    {
        string[] assayTypes = ["PCR", "ELISA", "LCMS", "SEQ", "IMG", "FLOW"];
        string[] states = ["OK", "WATCH", "CAL", "RUN", "QC"];

        return new LabSample
        {
            Id = $"SMP-{2400 + index:D4}",
            Assay = assayTypes[index % assayTypes.Length],
            State = states[(index + 2) % states.Length],
            BaseSignal = RandomRange(36f, 91f),
            Signal = RandomRange(36f, 91f),
            BaseTemperature = RandomRange(20.2f, 24.8f),
            Temperature = RandomRange(20.2f, 24.8f),
            BasePh = RandomRange(6.74f, 7.46f),
            Ph = RandomRange(6.74f, 7.46f),
            Speed = RandomRange(0.32f, 0.95f),
            Phase = RandomRange(0f, MathF.PI * 2f)
        };
    }

    /// <summary>
    /// Draws the dark green/blue laboratory-console background.
    /// </summary>
    private static void DrawBackground(Graphics graphics, RectangleF bounds)
    {
        using LinearGradientBrush brush = new(
            Rectangle.Round(bounds),
            Color.FromArgb(255, 1, 9, 12),
            Color.FromArgb(255, 2, 22, 30),
            LinearGradientMode.ForwardDiagonal);
        graphics.FillRectangle(brush, bounds);

        using SolidBrush vignette = new(Color.FromArgb(70, 0, 0, 0));
        graphics.FillRectangle(vignette, bounds);
    }

    /// <summary>
    /// Adds subtle scanlines so the console feels like a physical display.
    /// </summary>
    private static void DrawScanlines(Graphics graphics, RectangleF bounds)
    {
        using Pen scanlinePen = new(Color.FromArgb(18, 120, 255, 210), 1f);
        for (float y = bounds.Top; y < bounds.Bottom; y += 4f)
        {
            graphics.DrawLine(scanlinePen, bounds.Left, y, bounds.Right, y);
        }
    }

    /// <summary>
    /// Draws the main terminal-style frame.
    /// </summary>
    private static void DrawOuterFrame(Graphics graphics, RectangleF bounds)
    {
        using Pen glow = new(Color.FromArgb(55, 80, 245, 210), 6f);
        using Pen frame = new(Color.FromArgb(190, 95, 250, 210), 1.7f);
        graphics.DrawRectangle(glow, Rectangle.Round(bounds));
        graphics.DrawRectangle(frame, Rectangle.Round(bounds));
    }

    /// <summary>
    /// Draws the title and short live status line.
    /// </summary>
    private void DrawHeader(Graphics graphics, RectangleF bounds, Font titleFont, Font smallFont)
    {
        using SolidBrush titleBrush = new(Color.FromArgb(235, 175, 255, 222));
        using SolidBrush dimBrush = new(Color.FromArgb(155, 115, 225, 196));
        using Pen separator = new(Color.FromArgb(95, 95, 250, 210), 1f);

        DrawText(graphics, "SASD LAB CONSOLE // RESEARCH DATA STREAM", titleFont, titleBrush, bounds, StringAlignment.Near, StringAlignment.Near);

        string status = $"SESSION: SIM-{Math.Abs(MathF.Sin(_time * 0.09f)) * 9999:0000}   MODE: OBSERVE   CLOCK: {DateTime.Now:HH:mm:ss}   DATA: SYNTHETIC DEMO";
        RectangleF statusBounds = new(bounds.Left, bounds.Top + titleFont.GetHeight(graphics) + 7f, bounds.Width, smallFont.GetHeight(graphics) + 4f);
        DrawText(graphics, status, smallFont, dimBrush, statusBounds, StringAlignment.Near, StringAlignment.Near);

        graphics.DrawLine(separator, bounds.Left, bounds.Bottom - 2f, bounds.Right, bounds.Bottom - 2f);
    }

    /// <summary>
    /// Draws the main generated sample table.
    /// </summary>
    private void DrawSampleTable(Graphics graphics, RectangleF bounds, Font monoFont, Font smallFont, Font tinyFont)
    {
        DrawPanel(graphics, bounds, "SAMPLE QUEUE", smallFont);

        float y = bounds.Top + 34f;
        float lineHeight = Math.Max(18f, monoFont.GetHeight(graphics) + 5f);
        float bottomLimit = bounds.Bottom - 14f;

        using SolidBrush headerBrush = new(Color.FromArgb(190, 120, 255, 220));
        using SolidBrush normalBrush = new(Color.FromArgb(220, 185, 255, 225));
        using SolidBrush warnBrush = new(Color.FromArgb(220, 255, 208, 120));
        using SolidBrush dimBrush = new(Color.FromArgb(145, 120, 215, 190));
        using Pen barBackPen = new(Color.FromArgb(55, 90, 210, 180), 4f);
        using Pen barPen = new(Color.FromArgb(190, 130, 255, 215), 4f);

        RectangleF header = new(bounds.Left + 14f, y, bounds.Width - 28f, lineHeight);
        DrawText(graphics, "ID        TYPE   TEMP     pH    SIGNAL       STATE", tinyFont, headerBrush, header, StringAlignment.Near, StringAlignment.Near);
        y += lineHeight;

        int firstSample = (int)(_time * 0.18f) % Math.Max(1, _samples.Count);
        int rowsThatFit = Math.Max(3, (int)((bottomLimit - y) / lineHeight));

        for (int visibleIndex = 0; visibleIndex < rowsThatFit; visibleIndex++)
        {
            LabSample sample = _samples[(firstSample + visibleIndex) % _samples.Count];
            bool warning = sample.State is "WATCH" or "CAL";
            Brush textBrush = warning ? warnBrush : normalBrush;

            float rowTop = y + visibleIndex * lineHeight;
            RectangleF textBounds = new(bounds.Left + 14f, rowTop, bounds.Width * 0.72f, lineHeight);
            string row = $"{sample.Id,-9} {sample.Assay,-5} {sample.Temperature,5:0.0}C  {sample.Ph,4:0.00}  {sample.Signal,5:0.0}%   {sample.State}";
            DrawText(graphics, row, monoFont, textBrush, textBounds, StringAlignment.Near, StringAlignment.Near);

            float barLeft = bounds.Right - bounds.Width * 0.24f;
            float barRight = bounds.Right - 16f;
            float barY = rowTop + lineHeight * 0.56f;
            graphics.DrawLine(barBackPen, barLeft, barY, barRight, barY);
            graphics.DrawLine(barPen, barLeft, barY, barLeft + (barRight - barLeft) * (sample.Signal / 100f), barY);
        }

        RectangleF noteBounds = new(bounds.Left + 14f, bounds.Bottom - tinyFont.GetHeight(graphics) - 12f, bounds.Width - 28f, tinyFont.GetHeight(graphics) + 4f);
        DrawText(graphics, "NOTE: generated demonstration data - no patient or laboratory records are used", tinyFont, dimBrush, noteBounds, StringAlignment.Near, StringAlignment.Near);
    }

    /// <summary>
    /// Draws the signal charts and small summary cards on the right side.
    /// </summary>
    private void DrawSignalPanels(Graphics graphics, RectangleF bounds, Font monoFont, Font smallFont, Font tinyFont)
    {
        float gap = 14f;
        float topHeight = bounds.Height * 0.44f;
        RectangleF signalBounds = new(bounds.Left, bounds.Top, bounds.Width, topHeight);
        RectangleF metricBounds = new(bounds.Left, signalBounds.Bottom + gap, bounds.Width, bounds.Height - topHeight - gap);

        DrawPanel(graphics, signalBounds, "SIGNAL TRACE", smallFont);
        DrawSparkline(graphics, _primarySignal.ToArray(), new RectangleF(signalBounds.Left + 18f, signalBounds.Top + 44f, signalBounds.Width - 36f, (signalBounds.Height - 64f) * 0.46f), Color.FromArgb(210, 160, 255, 222));
        DrawSparkline(graphics, _secondarySignal.ToArray(), new RectangleF(signalBounds.Left + 18f, signalBounds.Top + 52f + (signalBounds.Height - 64f) * 0.48f, signalBounds.Width - 36f, (signalBounds.Height - 64f) * 0.40f), Color.FromArgb(190, 255, 210, 135));

        RectangleF labelA = new(signalBounds.Left + 18f, signalBounds.Top + 27f, signalBounds.Width - 36f, tinyFont.GetHeight(graphics) + 3f);
        using SolidBrush labelBrush = new(Color.FromArgb(170, 130, 235, 205));
        DrawText(graphics, "CH-A: fluorescence baseline     CH-B: absorbance derivative", tinyFont, labelBrush, labelA, StringAlignment.Near, StringAlignment.Near);

        DrawPanel(graphics, metricBounds, "BATCH SUMMARY", smallFont);
        DrawMetricLine(graphics, metricBounds, monoFont, 0, "THROUGHPUT", 72f + MathF.Sin(_time * 0.45f) * 11f, Color.FromArgb(210, 120, 255, 220));
        DrawMetricLine(graphics, metricBounds, monoFont, 1, "QUALITY", 88f + MathF.Sin(_time * 0.32f + 2f) * 5f, Color.FromArgb(220, 255, 225, 135));
        DrawMetricLine(graphics, metricBounds, monoFont, 2, "STABILITY", 81f + MathF.Sin(_time * 0.24f + 4f) * 7f, Color.FromArgb(210, 130, 220, 255));
    }

    /// <summary>
    /// Draws the scrolling event/status log at the bottom.
    /// </summary>
    private void DrawEventLog(Graphics graphics, RectangleF bounds, Font smallFont, Font tinyFont)
    {
        DrawPanel(graphics, bounds, "EVENT LOG", smallFont);

        using SolidBrush normalBrush = new(Color.FromArgb(210, 180, 255, 225));
        using SolidBrush prefixBrush = new(Color.FromArgb(170, 255, 210, 135));

        float lineHeight = Math.Max(14f, tinyFont.GetHeight(graphics) + 3f);
        int lines = Math.Max(1, (int)((bounds.Height - 32f) / lineHeight));
        int start = (int)_eventScroll % _eventMessages.Length;

        for (int index = 0; index < lines; index++)
        {
            string timestamp = DateTime.Now.AddSeconds(-(lines - index) * 7).ToString("HH:mm:ss");
            string message = _eventMessages[(start + index) % _eventMessages.Length];
            RectangleF lineBounds = new(bounds.Left + 14f, bounds.Top + 28f + index * lineHeight, bounds.Width - 28f, lineHeight + 2f);
            DrawText(graphics, $"[{timestamp}]", tinyFont, prefixBrush, new RectangleF(lineBounds.Left, lineBounds.Top, 78f, lineBounds.Height), StringAlignment.Near, StringAlignment.Near);
            DrawText(graphics, message, tinyFont, normalBrush, new RectangleF(lineBounds.Left + 82f, lineBounds.Top, lineBounds.Width - 82f, lineBounds.Height), StringAlignment.Near, StringAlignment.Near);
        }
    }

    /// <summary>
    /// Draws a framed panel with a small title tab.
    /// </summary>
    private static void DrawPanel(Graphics graphics, RectangleF bounds, string title, Font titleFont)
    {
        using Pen border = new(Color.FromArgb(115, 90, 245, 205), 1.2f);
        using Pen glow = new(Color.FromArgb(34, 90, 245, 205), 5f);
        using SolidBrush titleBrush = new(Color.FromArgb(210, 140, 255, 220));
        using SolidBrush backgroundBrush = new(Color.FromArgb(34, 0, 16, 18));

        graphics.FillRectangle(backgroundBrush, bounds);
        graphics.DrawRectangle(glow, Rectangle.Round(bounds));
        graphics.DrawRectangle(border, Rectangle.Round(bounds));

        RectangleF titleBounds = new(bounds.Left + 12f, bounds.Top + 6f, bounds.Width - 24f, titleFont.GetHeight(graphics) + 4f);
        DrawText(graphics, title, titleFont, titleBrush, titleBounds, StringAlignment.Near, StringAlignment.Near);
    }

    /// <summary>
    /// Draws one small metric with a progress bar.
    /// </summary>
    private static void DrawMetricLine(Graphics graphics, RectangleF bounds, Font font, int rowIndex, string label, float value, Color color)
    {
        value = Math.Clamp(value, 0f, 100f);
        float lineHeight = Math.Max(22f, font.GetHeight(graphics) + 10f);
        float y = bounds.Top + 40f + rowIndex * lineHeight * 1.35f;
        RectangleF textBounds = new(bounds.Left + 16f, y, bounds.Width * 0.42f, lineHeight);
        RectangleF valueBounds = new(bounds.Right - 70f, y, 54f, lineHeight);

        using SolidBrush textBrush = new(Color.FromArgb(220, color.R, color.G, color.B));
        DrawText(graphics, label, font, textBrush, textBounds, StringAlignment.Near, StringAlignment.Center);
        DrawText(graphics, $"{value:0}%", font, textBrush, valueBounds, StringAlignment.Far, StringAlignment.Center);

        float barLeft = bounds.Left + bounds.Width * 0.45f;
        float barRight = bounds.Right - 78f;
        float barY = y + lineHeight * 0.52f;
        using Pen backPen = new(Color.FromArgb(50, color.R, color.G, color.B), 5f);
        using Pen valuePen = new(Color.FromArgb(195, color.R, color.G, color.B), 5f);
        graphics.DrawLine(backPen, barLeft, barY, barRight, barY);
        graphics.DrawLine(valuePen, barLeft, barY, barLeft + (barRight - barLeft) * (value / 100f), barY);
    }

    /// <summary>
    /// Draws one simple sparkline in the supplied bounds.
    /// </summary>
    private static void DrawSparkline(Graphics graphics, float[] values, RectangleF bounds, Color color)
    {
        if (values.Length < 2 || bounds.Width <= 1f || bounds.Height <= 1f)
        {
            return;
        }

        using Pen gridPen = new(Color.FromArgb(35, color.R, color.G, color.B), 1f);
        for (int i = 0; i <= 4; i++)
        {
            float y = bounds.Top + bounds.Height * (i / 4f);
            graphics.DrawLine(gridPen, bounds.Left, y, bounds.Right, y);
        }

        PointF[] points = new PointF[values.Length];
        for (int index = 0; index < values.Length; index++)
        {
            float x = bounds.Left + bounds.Width * index / (values.Length - 1);
            float y = bounds.Bottom - bounds.Height * Math.Clamp(values[index], 0f, 1f);
            points[index] = new PointF(x, y);
        }

        using Pen glow = new(Color.FromArgb(60, color.R, color.G, color.B), 5f);
        using Pen line = new(Color.FromArgb(220, color.R, color.G, color.B), 1.7f);
        graphics.DrawLines(glow, points);
        graphics.DrawLines(line, points);
    }

    /// <summary>
    /// Draws clipped text with ellipsis so long strings never bleed into other panels.
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

    /// <summary>
    /// Enqueues one signal value and keeps the history bounded.
    /// </summary>
    private static void EnqueueSignal(Queue<float> queue, float value)
    {
        queue.Enqueue(value);
        while (queue.Count > SignalPointCount)
        {
            queue.Dequeue();
        }
    }

    /// <summary>
    /// Creates a normalized signal value.
    /// </summary>
    private static float CreateSignalValue(float t, float phase)
    {
        float value = 0.52f
            + MathF.Sin(t * 7.1f + phase) * 0.20f
            + MathF.Sin(t * 16.7f - phase * 0.4f) * 0.09f
            + MathF.Cos(t * 3.3f + phase * 1.8f) * 0.08f;
        return Math.Clamp(value, 0.05f, 0.98f);
    }

    /// <summary>
    /// Returns a deterministic random value within the requested range.
    /// </summary>
    private float RandomRange(float min, float max)
    {
        return min + (float)_random.NextDouble() * (max - min);
    }

    /// <summary>
    /// Holds generated sample state for the console table.
    /// </summary>
    private sealed class LabSample
    {
        public string Id { get; init; } = string.Empty;

        public string Assay { get; init; } = string.Empty;

        public string State { get; init; } = string.Empty;

        public float BaseSignal { get; init; }

        public float Signal { get; set; }

        public float BaseTemperature { get; init; }

        public float Temperature { get; set; }

        public float BasePh { get; init; }

        public float Ph { get; set; }

        public float Speed { get; init; }

        public float Phase { get; init; }
    }
}
