using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Sasd.ScreenSaverLab.Core;

namespace Sasd.ScreenSaverLab.App;

/// <summary>
/// Application context that can host one fullscreen screensaver window per monitor.
/// </summary>
/// <remarks>
/// A normal <c>Application.Run(form)</c> call is enough for one window, but multi-monitor
/// screensavers need several top-level windows that should close together. This context
/// keeps track of all windows and shuts the whole application down when the user exits
/// the screensaver from any monitor.
/// </remarks>
public sealed class ScreenSaverApplicationContext : ApplicationContext
{
    private readonly List<ScreenSaverForm> _forms = [];
    private bool _isClosingAllForms;

    /// <summary>
    /// Creates a new multi-window screensaver context.
    /// </summary>
    /// <param name="screens">The screens that should receive a fullscreen host window.</param>
    /// <param name="effectFactory">Factory that creates one independent effect instance per screen.</param>
    /// <param name="clock">Clock abstraction used for overlays.</param>
    public ScreenSaverApplicationContext(
        IEnumerable<Screen> screens,
        Func<IScreenSaverEffect> effectFactory,
        IEffectClock clock)
    {
        ArgumentNullException.ThrowIfNull(screens);
        ArgumentNullException.ThrowIfNull(effectFactory);
        ArgumentNullException.ThrowIfNull(clock);

        foreach (Screen screen in screens)
        {
            ScreenSaverForm form = new(effectFactory(), clock, screen, RequestExit);
            form.FormClosed += Form_FormClosed;
            _forms.Add(form);
        }

        if (_forms.Count == 0)
        {
            throw new InvalidOperationException("At least one screen is required to start the screensaver.");
        }

        foreach (ScreenSaverForm form in _forms)
        {
            form.Show();
        }
    }

    /// <summary>
    /// Requests a clean shutdown of all screensaver windows.
    /// </summary>
    private void RequestExit()
    {
        if (_isClosingAllForms)
        {
            return;
        }

        _isClosingAllForms = true;

        foreach (ScreenSaverForm form in _forms.ToArray())
        {
            if (!form.IsDisposed)
            {
                form.Close();
            }
        }
    }

    /// <summary>
    /// Removes closed windows from the list and exits the application when none are left.
    /// </summary>
    private void Form_FormClosed(object? sender, FormClosedEventArgs e)
    {
        if (sender is ScreenSaverForm form)
        {
            form.FormClosed -= Form_FormClosed;
            _forms.Remove(form);
        }

        if (_forms.Count == 0)
        {
            ExitThread();
        }
    }
}
