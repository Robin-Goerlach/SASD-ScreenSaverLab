using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Sasd.ScreenSaverLab.Core;
using Sasd.ScreenSaverLab.Effects;

namespace Sasd.ScreenSaverLab.App;

internal static class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        ScreenSaverStartupOptions options = ScreenSaverCommandLineParser.Parse(args);

        if (options.Mode == ScreenSaverMode.Configure)
        {
            MessageBox.Show(
                "SASD ScreenSaver Lab V0.1.3\n\nA graphical configuration dialog will be added in a later version.\n\nFor now, use command-line options such as /clock or /no-clock.",
                "SASD ScreenSaver Lab",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        if (options.Mode == ScreenSaverMode.Preview)
        {
            MessageBox.Show(
                "Preview mode is parsed but not implemented yet.\n\nPlease run the application normally for the V0.1.3 prototype.",
                "SASD ScreenSaver Lab",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        IReadOnlyList<Screen> targetScreens = ScreenSelector.SelectScreens(options);

        // Each screen gets its own effect instance. Sharing one effect object between
        // several windows would mix their particle state and screen sizes.
        Application.Run(new ScreenSaverApplicationContext(
            targetScreens,
            effectFactory: static () => new StarDriftEffect(),
            clock: new SystemEffectClock(),
            showClockOverlay: options.ShowClockOverlay));
    }
}
