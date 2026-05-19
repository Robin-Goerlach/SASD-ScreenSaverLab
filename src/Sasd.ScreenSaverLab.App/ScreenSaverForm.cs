using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sasd.ScreenSaverLab.Core;
namespace Sasd.ScreenSaverLab.App;

/// <summary>
/// Main fullscreen host form for one screensaver effect on one monitor.
/// </summary>
/// <remarks>
/// The form deliberately contains only host behavior: fullscreen setup, input handling,
/// update timing, painting, and a small clock overlay. The visual effect itself is
/// delegated to <see cref="IScreenSaverEffect" />.
/// </remarks>
public sealed class ScreenSaverForm : Form
{
    private const int TargetFrameIntervalMs = 16;
    private const int MouseExitThresholdPixels = 12;

    private readonly IScreenSaverEffect _effect;
    private readonly IEffectClock _clock;
    private readonly Screen _targetScreen;
    private readonly bool _showClockOverlay;
    private readonly Action _requestExit;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly Stopwatch _stopwatch = new();

    private Point? _initialMousePosition;
    private bool _isInitialized;
    private bool _cursorWasHiddenByThisForm;

    /// <summary>
    /// Initializes a new screensaver host form.
    /// </summary>
    /// <param name="effect">The visual effect rendered by this host.</param>
    /// <param name="clock">Clock abstraction used by the overlay.</param>
    /// <param name="targetScreen">The monitor on which this form should be displayed.</param>
    /// <param name="showClockOverlay">
    /// When true, the form draws the built-in clock/date/effect-name overlay.
    /// When false, only the active visual effect is rendered.
    /// </param>
    /// <param name="requestExit">
    /// Callback used to close all screensaver windows. This is important when the app
    /// runs on several monitors at the same time.
    /// </param>
    public ScreenSaverForm(
        IScreenSaverEffect effect,
        IEffectClock clock,
        Screen targetScreen,
        bool showClockOverlay,
        Action requestExit)
    {
        _effect = effect ?? throw new ArgumentNullException(nameof(effect));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _targetScreen = targetScreen ?? throw new ArgumentNullException(nameof(targetScreen));
        _showClockOverlay = showClockOverlay;
        _requestExit = requestExit ?? throw new ArgumentNullException(nameof(requestExit));

        _timer = new System.Windows.Forms.Timer
        {
            Interval = TargetFrameIntervalMs
        };

        _timer.Tick += Timer_Tick;

        ConfigureWindow();
        ConfigureRendering();
    }

    /// <summary>
    /// Configures the form so it behaves like a fullscreen screensaver window.
    /// </summary>
    private void ConfigureWindow()
    {
        Text = "SASD ScreenSaver Lab";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;

        // Important for multi-monitor support:
        // Do not use WindowState = Maximized here. Windows often maximizes a form on
        // the primary monitor. A screensaver window should instead be placed explicitly
        // on the target monitor's bounds.
        WindowState = FormWindowState.Normal;
        Bounds = _targetScreen.Bounds;

        TopMost = true;
        KeyPreview = true;
        ShowInTaskbar = false;
        BackColor = Color.Black;
    }

    /// <summary>
    /// Enables double buffering and redraw behavior for smoother animation.
    /// </summary>
    private void ConfigureRendering()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        UpdateStyles();
    }

    /// <inheritdoc />
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // Re-apply the bounds after loading. This makes the placement more robust when
        // Windows Forms or DPI handling adjusts the form during creation.
        Bounds = _targetScreen.Bounds;

        Cursor.Hide();
        _cursorWasHiddenByThisForm = true;
        _initialMousePosition = Cursor.Position;

        _effect.Initialize(ClientSize);

        _stopwatch.Restart();
        _timer.Start();
        _isInitialized = true;
    }

    /// <inheritdoc />
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        // Make sure the fullscreen window covers the selected screen even if Windows
        // changed the size while showing the form.
        Bounds = _targetScreen.Bounds;
        BringToFront();
    }

    /// <inheritdoc />
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timer.Stop();
        _timer.Tick -= Timer_Tick;
        _timer.Dispose();

        if (_cursorWasHiddenByThisForm)
        {
            Cursor.Show();
            _cursorWasHiddenByThisForm = false;
        }

        base.OnFormClosed(e);
    }

    /// <inheritdoc />
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (_isInitialized)
        {
            // Reinitialize the effect after a resize. This keeps V0.1.3 simple and avoids
            // partially off-screen particles after monitor or window size changes.
            _effect.Initialize(ClientSize);
        }
    }

    /// <inheritdoc />
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        _effect.Render(e.Graphics, ClientSize);
        DrawOverlay(e.Graphics);
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _requestExit();
    }

    /// <inheritdoc />
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _requestExit();
    }

    /// <inheritdoc />
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_isInitialized || _initialMousePosition is null)
        {
            return;
        }

        Point current = Cursor.Position;
        Point initial = _initialMousePosition.Value;

        int dx = current.X - initial.X;
        int dy = current.Y - initial.Y;
        double distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance >= MouseExitThresholdPixels)
        {
            _requestExit();
        }
    }

    /// <summary>
    /// Updates the active effect and schedules the next repaint.
    /// </summary>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        TimeSpan elapsed = _stopwatch.Elapsed;
        _stopwatch.Restart();

        _effect.Update(elapsed, ClientSize);
        Invalidate();
    }

    /// <summary>
    /// Draws small informational overlay text if the clock overlay is enabled.
    /// </summary>
    private void DrawOverlay(Graphics graphics)
    {
        if (!_showClockOverlay)
        {
            return;
        }

        DateTime now = _clock.Now;

        string timeText = now.ToString("HH:mm");
        string dateText = now.ToString("dddd, dd. MMMM yyyy");
        string effectText = _effect.Name;

        using Font timeFont = new("Segoe UI", 36f, FontStyle.Regular, GraphicsUnit.Point);
        using Font dateFont = new("Segoe UI", 11f, FontStyle.Regular, GraphicsUnit.Point);
        using Font effectFont = new("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);

        using SolidBrush timeBrush = new(Color.FromArgb(220, 235, 245, 255));
        using SolidBrush secondaryBrush = new(Color.FromArgb(145, 210, 225, 240));

        SizeF timeSize = graphics.MeasureString(timeText, timeFont);
        SizeF dateSize = graphics.MeasureString(dateText, dateFont);
        SizeF effectSize = graphics.MeasureString(effectText, effectFont);

        float margin = 32f;
        float x = ClientSize.Width - Math.Max(timeSize.Width, dateSize.Width) - margin;
        float y = ClientSize.Height - timeSize.Height - dateSize.Height - effectSize.Height - margin;

        graphics.DrawString(effectText, effectFont, secondaryBrush, x, y);
        graphics.DrawString(timeText, timeFont, timeBrush, x, y + effectSize.Height + 2);
        graphics.DrawString(dateText, dateFont, secondaryBrush, x, y + effectSize.Height + timeSize.Height + 4);
    }
}
