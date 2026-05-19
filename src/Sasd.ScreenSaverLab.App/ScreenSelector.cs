using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Sasd.ScreenSaverLab.Core;

namespace Sasd.ScreenSaverLab.App;

/// <summary>
/// Resolves startup options into concrete Windows Forms screen objects.
/// </summary>
public static class ScreenSelector
{
    /// <summary>
    /// Selects the screen or screens on which the screensaver should run.
    /// </summary>
    /// <param name="options">Parsed startup options.</param>
    /// <returns>A non-empty screen list.</returns>
    public static IReadOnlyList<Screen> SelectScreens(ScreenSaverStartupOptions options)
    {
        Screen[] allScreens = Screen.AllScreens;

        if (allScreens.Length == 0)
        {
            // This is highly unlikely on Windows, but returning the primary screen keeps
            // the caller code simple and documents the intended fallback.
            return [Screen.PrimaryScreen ?? Screen.FromPoint(Cursor.Position)];
        }

        if (options.UseAllScreens)
        {
            return allScreens;
        }

        if (options.UsePrimaryScreen)
        {
            return [Screen.PrimaryScreen ?? allScreens[0]];
        }

        if (options.TargetScreenIndex is int screenIndex)
        {
            return [allScreens[Math.Clamp(screenIndex, 0, allScreens.Length - 1)]];
        }

        if (options.UseMouseScreen)
        {
            return [Screen.FromPoint(Cursor.Position)];
        }

        return [Screen.PrimaryScreen ?? allScreens[0]];
    }
}
